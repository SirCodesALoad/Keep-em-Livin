using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BlackGardenStudios.HitboxStudioPro
{
    public class SheetAnimatorEditor : EditorWindow
    {
        Texture2D SelectedSheet;
        ICharacter SelectedCharacter;
        HitboxManager SelectedManager;
        SpritePalette SelectedPalette;
        Sprite[] SelectedSprites;
        NewAnimation.Keyframe DragSprite;

        float toolbarWidth = 300f;
        float previewScroll = 0f;
        float verticalScroll = 0f;
        int mouseOverIndex = -1;
        
        List<NewAnimation> Animations = new List<NewAnimation>();

        GameObject target;
        SerializedObject m_Object;

        //static internal dictionary of input texture -> output textures that were processed by palettes if one was provided
        static internal Dictionary<string, Texture2D> TextureSwap = new Dictionary<string, Texture2D>();

        static Texture2D editorBackgroundImage;

        static internal Rect RenderSprite(Sprite sprite, float x, float y, float w, float h)
        {
            return RenderSprite(sprite, new Vector2(x, y), new Vector2(w, h));
        }

        static internal Rect RenderSprite(Sprite sprite, Vector2 position, Vector2 size)
        {
            var previewRect = new Rect(position, size);
            var spriteRect = sprite.rect;
            var sheet = sprite.texture;
            Rect atlasRect = new Rect(spriteRect.x / sheet.width, spriteRect.y / sheet.height,
                   spriteRect.width / sheet.width, spriteRect.height / sheet.height);
            var grid = size / 4f;
            var gridscale = size.y / 128f;
            var offset = Vector2.zero;

            if (spriteRect.width > spriteRect.height)
            {
                previewRect.height = spriteRect.height / spriteRect.width * previewRect.width;
                offset.y += (previewRect.width - previewRect.height) / 2f;
            }
            else
            {
                previewRect.width = spriteRect.width / spriteRect.height * previewRect.height;
                offset.x += (previewRect.height - previewRect.width) / 2f;
            }

            for (int x = 0; x < 4; x++)
                for (int y = 0; y < 4; y++)
                    GUI.DrawTexture(new Rect(previewRect.x + x * grid.x, previewRect.y + y * grid.y, grid.x, grid.y), editorBackgroundImage);

            if (!TextureSwap.TryGetValue(sprite.texture.name, out sheet)) sheet = sprite.texture;
            GUI.DrawTextureWithTexCoords(new Rect(previewRect.position + offset, previewRect.size), sheet, atlasRect);
            HitBoxManagerInspector.DrawLabel(sprite.name, new Vector2(position.x + 4, position.y + 114 * gridscale), Mathf.RoundToInt(10 * gridscale), FontStyle.Italic);

            return previewRect;
        }

        internal class NewAnimation
        {
            internal class Keyframe
            {
                public Sprite sprite;
                public int length;

                public Keyframe(Sprite Sprite, int Length)
                {
                    sprite = Sprite;
                    length = Length;
                }
            }

            public List<Keyframe> frame = new List<Keyframe>();
            public int fps = 24;
            public bool loop = false;
            public string name;
            SpritePalette palette;
            float scroll = 0f;
            Vector2 lastPosition;
            int currentFrame;
            SheetAnimatorEditor editor;
            AnimationClip clip;
            bool playing = false;
            double nextTime = 0f;
            public Rect InsertRect
            {
                get
                {
                    var pos = new Vector2(138, 24) + lastPosition;
                    return new Rect(pos, new Vector2(EditorGUIUtility.currentViewWidth - pos.x, 128));
                }
            }

            public NewAnimation(SheetAnimatorEditor Editor)
            {
                editor = Editor;
                palette = editor.SelectedPalette;
            }

            public NewAnimation(SheetAnimatorEditor Editor, AnimationClip Clip)
            {
                editor = Editor;
                palette = editor.SelectedPalette;
                Load(Clip);
            }

            public Keyframe Render(Vector2 position, Event currentEvent, ref EventType type, Action addAll, Action remove, float rightPadding = 0f)
            {
                var animWidth = frame.Count * 134f;
                var viewWidth = EditorGUIUtility.currentViewWidth - position.x - 134 - rightPadding;
                Keyframe drag = null;

                lastPosition = position;
                GUI.enabled = false;
                var scrollView = new Rect(position.x + 134, position.y, viewWidth, 174f);
                GUI.Button(scrollView, "");
                GUI.enabled = true;

                scrollView.position += new Vector2(4, 0);
                scrollView.size -= new Vector2(8, 0);

                GUI.BeginScrollView(scrollView, Vector2.zero, scrollView);

                if (type == EventType.Repaint || type == EventType.MouseDown)
                    for (int i = 0; i < frame.Count; i++)
                    {
                        var rect = RenderSprite(frame[i].sprite, position.x + 138 + scroll * -animWidth + i * 134f, position.y + 24, 128, 128);

                        if(type == EventType.MouseDown && rect.Contains(currentEvent.mousePosition) && drag == null)
                        {
                            drag = frame[i];
                            frame.RemoveAt(i);
                            i--;
                        }
                    }

                for (int i = 0; i < frame.Count; i++)
                {
                    frame[i].length = EditorGUI.IntField(new Rect(position.x + 138 + scroll * -animWidth + i * 134f + 112, position.y + 24 + 112, 16, 16), frame[i].length);
                    EditorGUIUtility.AddCursorRect(new Rect(position.x + 138 + scroll * -animWidth + i * 134f, position.y + 24, 128, 128), MouseCursor.Pan);
                }

                GUI.EndScrollView();

                name = EditorGUI.TextField(new Rect(position + new Vector2(134 + 40, 4), new Vector2(164, 16)), name);
                if (GUI.Button(new Rect(position + new Vector2(134 + 204, 4), new Vector2(80, 16)), "Add All"))
                {
                    addAll();
                    if (palette != null)
                        ProcessSheets(frame.Select((Keyframe keyframe) => keyframe.sprite.texture).ToArray(), palette);
                }
                if (GUI.Button(new Rect(position + new Vector2(134 + 284, 4), new Vector2(80, 16)), "Clear")) frame.Clear();
                if (clip != null && GUI.Button(new Rect(position + new Vector2(134 + 364, 4), new Vector2(80, 16)), "Save")) Save(clip);
                if (clip != null && GUI.Button(new Rect(position + new Vector2(134 + 444, 4), new Vector2(80, 16)), "Save As")) SaveAs();
                if (clip == null && GUI.Button(new Rect(position + new Vector2(134 + 364, 4), new Vector2(80, 16)), "Save As")) SaveAs();
                var icnSz = EditorGUIUtility.GetIconSize();
                EditorGUIUtility.SetIconSize(new Vector2(10, 10));
                if (GUI.Button(new Rect(position + new Vector2(114 + viewWidth, 4), new Vector2(16, 16)), EditorGUIUtility.IconContent(EditorGUIUtility.isProSkin ? "d_winbtn_win_close" : "winbtn_win_close")))
                {
                    Stop();
                    remove();
                }
                EditorGUIUtility.SetIconSize(icnSz);
                fps = EditorGUI.IntField(new Rect(position + new Vector2(134 + 28, 154), new Vector2(24, 16)), fps);
                loop = EditorGUI.Toggle(new Rect(position + new Vector2(134 + 28 + 58, 153), new Vector2(16, 16)), loop);

                if (animWidth > viewWidth)
                {
                    if (type == EventType.ScrollWheel && scrollView.Contains(currentEvent.mousePosition))
                    {
                        scroll += currentEvent.delta.y * 16 / animWidth;
                        type = EventType.Used;
                        editor.Repaint();
                    }

                    scroll = GUI.HorizontalScrollbar(
                          new Rect(position.x + 134 + 28 + 58 + 16,
                          position.y + 154,
                          viewWidth - (28 + 58 + 20),
                          16), scroll, viewWidth / animWidth, 0f, 1f);
                }

                GUI.enabled = false;
                GUI.Button(new Rect(position.x, position.y, 134f, 174f), "");
                GUI.enabled = true;

                icnSz = EditorGUIUtility.GetIconSize();
                EditorGUIUtility.SetIconSize(new Vector2(10, 10));
                if (playing == false && GUI.Button(new Rect(53, position.y + 4, 16, 16), EditorGUIUtility.IconContent("PlayButton")))
                {
                    Play();
                }
                else if (playing == true && GUI.Button(new Rect(53, position.y + 4, 16, 16), EditorGUIUtility.IconContent("PauseButton")))
                {
                    Stop();
                }
                EditorGUIUtility.SetIconSize(icnSz);

                var oldPalette = palette;
                palette = (SpritePalette)EditorGUI.ObjectField(new Rect(4f, position.y + 154, 120, 16), palette, typeof(SpritePalette), false);
                if (palette != oldPalette && palette != null)
                    ProcessSheets(frame.Select((Keyframe keyframe) => keyframe.sprite.texture).ToArray(), palette);

                if (type == EventType.Repaint)
                {
                    HitBoxManagerInspector.DrawLabel("Name", position + new Vector2(138, 7), 10, FontStyle.Bold, true);
                    HitBoxManagerInspector.DrawLabel("FPS", position + new Vector2(138, 157), 10, FontStyle.Bold, true);
                    HitBoxManagerInspector.DrawLabel("Loop", position + new Vector2(138 + 28 + 24, 157), 10, FontStyle.Bold, true);
                    HitBoxManagerInspector.DrawLabel("Preview", position + new Vector2(4, 7), 10, FontStyle.Bold, true); 

                    //Preview
                    if (frame.Count > 0 && currentFrame < frame.Count)
                        RenderSprite(frame[currentFrame].sprite, position.x + 4, position.y + 24, 128, 128);
                }

                return drag;
            }

            public void Insert(Keyframe keyframe, float position, bool confirm)
            {
                var animWidth = frame.Count * 134f;
                var viewScroll = animWidth * scroll;

                position -= 138;
                position += viewScroll;

                int index = Mathf.FloorToInt(position / 134f);

                if(index >= frame.Count)
                {
                    EditorGUI.DrawRect(new Rect(frame.Count * 134f + 138, lastPosition.y + 24, EditorGUIUtility.currentViewWidth - (frame.Count * 134f + 142), 128f), new Color(1f, 1f, 1f, 0.5f));

                    if (confirm)
                        frame.Add(keyframe);
                }
                else
                {
                    position -= index * 134;

                    if(position <= 42)
                    {
                        EditorGUI.DrawRect(new Rect(index * 134f + 138 - viewScroll, lastPosition.y + 24, 42, 128), new Color(1f, 1f, 1f, 0.5f));
                        HitBoxManagerInspector.DrawLabel("<<", new Vector2(index * 134f + 148 - viewScroll, lastPosition.y + 75), 12, FontStyle.Bold, true);

                        if (confirm)
                            frame.Insert(index, keyframe);
                    }
                    else if(position <= 85)
                    {
                        EditorGUI.DrawRect(new Rect(index * 134f + 138 + 42 - viewScroll, lastPosition.y + 24, 43, 128), new Color(1f, 1f, 1f, 0.5f));
                        if (confirm)
                            frame[index] = keyframe;
                    }
                    else
                    {
                        EditorGUI.DrawRect(new Rect(index * 134f + 138 + 85 - viewScroll, lastPosition.y + 24, 42, 128), new Color(1f, 1f, 1f, 0.5f));
                        HitBoxManagerInspector.DrawLabel(">>", new Vector2(index * 134f + 148 + 85 - viewScroll, lastPosition.y + 75), 12, FontStyle.Bold, true);
                        if (confirm)
                            frame.Insert(index + 1, keyframe);
                    }
                }

                if (palette != null && !TextureSwap.ContainsKey(keyframe.sprite.texture.name))
                    ProcessSheets(new Texture2D[] { keyframe.sprite.texture }, palette);
            }

            void Preview()
            {
                if (EditorApplication.isPlaying && !EditorApplication.isPaused) return;
                if (frame.Count <= 0) return;
                var time = EditorApplication.timeSinceStartup;

                if (time >= nextTime)
                {
                    nextTime = time;
                    currentFrame++;
                    if(currentFrame >= frame.Count)
                    {
                        if (loop == false)
                        {
                            Stop();
                            currentFrame = frame.Count - 1;
                        }
                        else
                            currentFrame = 0;
                    }

                    nextTime = time + frame[currentFrame].length * (1.0 / fps);
                    editor.Repaint();
                }
            }

            void Play()
            {
                if (playing == false && frame.Count > 0)
                {
                    if (currentFrame >= frame.Count - 1)
                        currentFrame = 0;
                    nextTime = EditorApplication.timeSinceStartup + frame[currentFrame].length * (1f / fps);
                    EditorApplication.update += Preview;
                    playing = true;
                }
            }

            void Stop()
            {
                if (playing)
                {
                    EditorApplication.update -= Preview;
                    playing = false;
                }
            }

            void Save(AnimationClip clip)
            {

                var curve = new EditorCurveBinding();

                curve.type = typeof(SpriteRenderer);
                curve.propertyName = "m_Sprite";
                curve.path = "";
                clip.frameRate = fps;

                if (frame.Count == 0)
                {
                    AnimationUtility.SetObjectReferenceCurve(clip, curve, new ObjectReferenceKeyframe[] { });
                }
                else
                {
                    float lastTime = 0f;
                    float step = 1f / fps;
                    var copy = new List<Keyframe>(frame.Select((Keyframe keyframe) => new Keyframe(keyframe.sprite, keyframe.length)));
                    var lastFrame = copy[copy.Count - 1];

                    //The last frame can't actually hold a length greater than 1.
                    //So let's shave 1 length off and duplicate the frame at the end.
                    if(lastFrame.length > 1)
                    {
                        lastFrame.length--;
                        copy.Add(new Keyframe(lastFrame.sprite, 1));
                    }

                    AnimationUtility.SetObjectReferenceCurve(clip, curve, copy.Select((Keyframe keyframe) =>
                    {
                        var objectKeyframe = new ObjectReferenceKeyframe();

                        objectKeyframe.value = keyframe.sprite;
                        objectKeyframe.time = lastTime;
                        lastTime += step * Mathf.Max(1, keyframe.length);

                        return objectKeyframe;
                    }).ToArray());
                }

                var settings = AnimationUtility.GetAnimationClipSettings(clip);
                settings.loopTime = loop;
                AnimationUtility.SetAnimationClipSettings(clip, settings);
            }

            void SaveAs()
            {
                if (frame.Count > 0)
                {
                    var animationPath = EditorUtility.SaveFilePanel("Save animation as", null, name, "anim");

                    if (!string.IsNullOrEmpty(animationPath))
                    {
                        var indexOf = animationPath.IndexOf("Assets/");
                        animationPath = animationPath.Substring(indexOf >= 0 ? indexOf : 0);

                        var newClip = new AnimationClip();
                        Save(newClip);
                        clip = newClip;

                        AssetDatabase.CreateAsset(newClip, animationPath);
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newClip));
                        editor.AddAnimationToManager(newClip);
                    }
                }
            }

            void Load(AnimationClip Clip)
            {
                clip = Clip;
                fps = Mathf.RoundToInt(clip.frameRate);
                name = clip.name;
                loop = AnimationUtility.GetAnimationClipSettings(clip).loopTime;

                var binding = new EditorCurveBinding();
                binding.type = typeof(SpriteRenderer);
                binding.propertyName = "m_Sprite";
                binding.path = "";
                var keyframes = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                Keyframe lastFrame = new Keyframe(null, 0);

                if(keyframes != null)
                {
                    if (palette != null)
                        ProcessSheets(keyframes.Select((ObjectReferenceKeyframe o) => ((Sprite)o.value).texture).ToArray(), palette);

                    float lastTime = 0f;
                    float step = 1f / fps;

                    frame.AddRange(keyframes.OrderBy((ObjectReferenceKeyframe o) => o.time).Select((ObjectReferenceKeyframe o) =>
                    {
                        var keyframe = new Keyframe((Sprite)o.value, 1);

                        lastFrame.length = Mathf.Max(1, Mathf.RoundToInt((o.time - lastTime) / step));

                        lastTime = o.time;
                        lastFrame = keyframe;

                        return keyframe;
                    }));

                    if(frame.Count >= 2)
                    {
                        var a = frame[frame.Count - 2];
                        var b = frame[frame.Count - 1];

                        if(a.sprite == b.sprite)
                        {
                            a.length++;
                            frame.RemoveAt(frame.Count - 1);
                        }
                    }
                }
            }
        }

        [MenuItem("Window/Black Garden Studios/Animation Editor")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            SheetAnimatorEditor window = (SheetAnimatorEditor)EditorWindow.GetWindow(typeof(SheetAnimatorEditor), true, "Animation Editor");
            window.target = null;
            window.SelectedCharacter = null;
            window.SelectedPalette = null;
            window.SelectedManager = null;
            window.m_Object = null;
            TextureSwap.Clear();
            window.Show();
        }

        static public void Init(GameObject target, SerializedObject serialized)
        {
            // Get existing open window or if none, make a new one:
            SheetAnimatorEditor window = (SheetAnimatorEditor)EditorWindow.GetWindow(typeof(SheetAnimatorEditor), true, "Animation Editor");

            //Init private properties
            window.target = target;
            if(target != null)
            {
                window.SelectedCharacter = target.GetComponentInChildren<ICharacter>();
                if (window.SelectedCharacter != null)
                    window.SelectedPalette = window.SelectedCharacter.ActivePalette;
                window.SelectedManager = target.GetComponentInChildren<HitboxManager>();
            }
            else
            {
                window.SelectedCharacter = null;
                window.SelectedPalette = null;
                window.SelectedManager = null;
            }
            window.m_Object = serialized;
            TextureSwap.Clear();
            window.Show();
        }

        private void OnEnable()
        {
            var size = position.size;

            if (size.x < 800f)
                size.x = 800f;
            if (size.y < 134f * 3f)
                size.y = 134f * 6f;

            position = new Rect(position.position, size);
        }

        void OnGUI()
        {
            if (editorBackgroundImage == null)
                editorBackgroundImage = Resources.Load<Texture2D>("GrayCheckerBackground");
            
            wantsMouseMove = true;
            var currentEvent = Event.current;
            var eventType = currentEvent.type;
            var oldMouseOverIndex = mouseOverIndex;

            if ((eventType == EventType.MouseDrag && DragSprite != null) ||
                eventType == EventType.MouseDown ||
                eventType == EventType.MouseUp)
                Repaint();

            GUI.enabled = false;
            var scrollView = new Rect(0, 0, EditorGUIUtility.currentViewWidth, 154);
            GUI.Button(scrollView, "");
            GUI.enabled = true;

            if (SelectedSprites != null)
            {
                var offset = 4f;
                var scrollOffset = offset + toolbarWidth;
                var viewWidth = EditorGUIUtility.currentViewWidth - offset;
                var animWidth = SelectedSprites.Length * 134;

                if (animWidth > viewWidth)
                {
                    if (eventType == EventType.ScrollWheel && scrollView.Contains(currentEvent.mousePosition))
                    {
                        previewScroll += currentEvent.delta.y * 16 / animWidth;
                        currentEvent.delta = Vector2.zero;
                        Repaint();
                    }
                    offset += (previewScroll = GUI.HorizontalScrollbar(new Rect(scrollOffset, 4, viewWidth - scrollOffset, 16), previewScroll, viewWidth / animWidth, 0f, 1f)) * -animWidth;
                }

                mouseOverIndex = -1;

                for (int i = 0; i < SelectedSprites.Length; i++, offset += 134)
                {
                    var positionRect = new Rect(offset, 20, 128, 128);
                    var contains = positionRect.Contains(currentEvent.mousePosition);

                    if (eventType == EventType.MouseDown && contains)
                        DragSprite = new NewAnimation.Keyframe(SelectedSprites[i], 1);

                    if (contains)
                        mouseOverIndex = i;

                    EditorGUIUtility.AddCursorRect(positionRect, MouseCursor.Pan);

                    if(eventType == EventType.Repaint)
                    {
                        EditorGUI.DrawRect(new Rect(positionRect.x - 1,
                            positionRect.y - 1,
                            positionRect.width + 2,
                            positionRect.height + 2), positionRect.Contains(currentEvent.mousePosition) ? Color.white : Color.black);

                        RenderSprite(SelectedSprites[i], positionRect.position, positionRect.size);
                    }
                }
            }
            else if(SelectedSheet != null)
                GetSpriteMetaData();

            if (Animations != null)
            {
                int x = 0, y = 160;
                var viewHeight = position.height - y;
                var animHeight = Animations.Count * 174 + 128;
                scrollView = new Rect(0f, y, position.width, viewHeight);
                GUI.BeginScrollView(scrollView, Vector2.zero, scrollView);

                y -= Mathf.RoundToInt(verticalScroll * animHeight);
                
                for (int i = 0; i < Animations.Count; i++, y += 174)
                {
                    var anim = Animations[i];
                    var drag = anim.Render(new Vector2(x, y), currentEvent, ref eventType,
                        () => anim.frame.AddRange(SelectedSprites.Select((Sprite sprite) =>new NewAnimation.Keyframe(sprite, 1))),
                        () => { Animations.Remove(anim); i--; }, animHeight > viewHeight ? 16 : 0);

                    if (drag != null) DragSprite = drag;
                }

                if (GUI.Button(new Rect(4f, y, 60, 128), "New"))
                    Animations.Add(new NewAnimation(this));
                if (GUI.Button(new Rect(64f, y, 60, 128), "Load"))
                {
                    var path = EditorUtility.OpenFilePanel("Select Animation", null, "anim");

                   if (!string.IsNullOrEmpty(path))
                   {
                        int indexOf = path.IndexOf("Assets/");
                        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path.Substring(indexOf >= 0 ? indexOf : 0));

                        if (clip != null) Animations.Add(new NewAnimation(this, clip));
                   }
                }

                //Need to put this after I render all the animations incase one of the animations has captured the mousewheel event.
                if (animHeight > viewHeight)
                {
                    verticalScroll = GUI.VerticalScrollbar(new Rect(position.width - 16, scrollView.y, 16f, viewHeight), verticalScroll, viewHeight / animHeight, 0f, 1f);
                    if (eventType == EventType.ScrollWheel)
                    {
                        verticalScroll += currentEvent.delta.y * 16 / animHeight;
                        Repaint();
                    }
                }
                else
                    verticalScroll = 0f;

                GUI.EndScrollView();
            }

            if(DragSprite != null)
                for (int i = 0; i < Animations.Count; i++)
                    if (Animations[i].InsertRect.Contains(currentEvent.mousePosition))
                        Animations[i].Insert(DragSprite, currentEvent.mousePosition.x, eventType == EventType.MouseUp);
            if (eventType == EventType.MouseUp)
                DragSprite = null;
            else if(eventType == EventType.Repaint && DragSprite != null)
                RenderSprite(DragSprite.sprite, currentEvent.mousePosition, new Vector2(64f, 64f));

            EditorGUILayout.BeginHorizontal(GUILayout.Width(toolbarWidth));
            EditorGUILayout.LabelField("Select Sprite Sheet", GUILayout.Width(112f));
            var oldSheet = SelectedSheet;
            SelectedSheet = (Texture2D)EditorGUILayout.ObjectField(SelectedSheet, typeof(Texture2D), false);
            EditorGUILayout.EndHorizontal();

            if (oldSheet != SelectedSheet)
                GetSpriteMetaData();

            if (oldMouseOverIndex != mouseOverIndex)
                Repaint();
        }

        private void GetSpriteMetaData()
        {
            if (SelectedSheet != null)
            {
                var path = AssetDatabase.GetAssetPath(SelectedSheet);

                SelectedSprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();

                if (SelectedPalette != null)
                    ProcessSheets(SelectedSprites.Select((Sprite sprite) => sprite.texture).ToArray(), SelectedPalette);

                previewScroll = 0f;
            }
            else
                SelectedSprites = new Sprite[] { };
        }

        static internal void ProcessSheets(Texture2D[] texture, SpritePalette palette)
        {
            if (palette == null) return;

            List<string> alreadyProcessed = new List<string>();

            for (int i = 0; i < texture.Length; i++)
            {
                var tex = texture[i];
                
                if (!alreadyProcessed.Contains(tex.name))
                {
                    var copy = new Texture2D(tex.width, tex.height, tex.format, false);
                    copy.filterMode = tex.filterMode;

                    var pixels = tex.GetPixels();

                    for (int j = 0; j < pixels.Length; j++)
                    {
                        var index = Mathf.Clamp(Mathf.RoundToInt(pixels[j].r * 255f), 0, palette.Colors.Length - 1);
                        var alpha = pixels[j].a;
                        pixels[j] = palette.Colors[index];
                        pixels[j].a = alpha;
                    }

                    copy.SetPixels(pixels);
                    copy.Apply();
                    if (TextureSwap.ContainsKey(tex.name))
                        TextureSwap[tex.name] = copy;
                    else
                        TextureSwap.Add(tex.name, copy);
                    alreadyProcessed.Add(tex.name);
                }
            }
        }

        internal void AddAnimationToManager(AnimationClip loadedClip)
        {
            if (SelectedManager != null && m_Object != null && loadedClip != null)
            {
                var serializedAnimations = m_Object.FindProperty("m_Animations");

                for (int i = 0; i < serializedAnimations.arraySize; i++)
                {
                    var element = serializedAnimations.GetArrayElementAtIndex(i);
                    var elClip = element.FindPropertyRelative("clip");

                    //If the manager already contains the clip we should abort this operation.
                    if (((AnimationClip)elClip.objectReferenceValue) == loadedClip) return;
                }

                serializedAnimations.InsertArrayElementAtIndex(Mathf.Max(0, serializedAnimations.arraySize - 1));

                m_Object.ApplyModifiedProperties();

                SerializedProperty HitboxData = serializedAnimations.GetArrayElementAtIndex(serializedAnimations.arraySize - 1);
                var clip = HitboxData.FindPropertyRelative("clip");
                var framedata = HitboxData.FindPropertyRelative("framedata");

                clip.objectReferenceValue = loadedClip;
                for (int i = 0, j = SelectedManager.GetNumFrames(serializedAnimations.arraySize - 1); i < j; i++)
                {
                    framedata.InsertArrayElementAtIndex(0);
                }

                m_Object.ApplyModifiedProperties();
            }
        }
    }
}