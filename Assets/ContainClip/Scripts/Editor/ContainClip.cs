using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace ContainClip
{
    public class ContainClip : EditorWindow
    {
        private RuntimeAnimatorController _controller;
        private AnimationClip _baseAnimationClip;

        private string _clipName;

        [MenuItem("Assets/CombineAnimationClip")]
        private static void Create()
        {
            var window = GetWindow(typeof(ContainClip)) as ContainClip;
            if (!(Selection.activeObject is RuntimeAnimatorController)) return;
            if (window != null)
            {
                window._controller = (RuntimeAnimatorController)Selection.activeObject;
                window.titleContent = new GUIContent("ContainClip");
            }
        }

        private void OnGUI()
        {
            _controller = EditorGUILayout.ObjectField(
                _controller,
                typeof(RuntimeAnimatorController),
                false
            ) as RuntimeAnimatorController;

            if (_controller == null) return;

            var clipList = new List<AnimationClip>();

            var allAsset = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(_controller));
            foreach (var asset in allAsset)
            {
                var removeClip = asset as AnimationClip;
                if (removeClip == null) continue;
                if (!clipList.Contains(removeClip))
                {
                    clipList.Add(removeClip);
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Add new clip");
            EditorGUILayout.BeginVertical("box");

            _clipName = EditorGUILayout.TextField(_clipName);

            _baseAnimationClip = EditorGUILayout.ObjectField(
                _baseAnimationClip,
                typeof(AnimationClip),
                false
            ) as AnimationClip;

            if (_baseAnimationClip != null)
            {
                var clipName = string.IsNullOrEmpty(_clipName) ? _baseAnimationClip.name : _clipName;
                if (clipList.Exists(item => item.name == clipName))
                {
                    EditorGUILayout.LabelField("can't create duplicate names");
                }
                else
                {
                    if (GUILayout.Button("create"))
                    {
                        var animationClip = UnityEditor.Animations.AnimatorController.AllocateAnimatorClip(clipName);

                        var events = AnimationUtility.GetAnimationEvents(_baseAnimationClip);
                        var srcClipInfo = AnimationUtility.GetAnimationClipSettings(_baseAnimationClip);
                        AnimationUtility.SetAnimationEvents(animationClip, events);
                        AnimationUtility.SetAnimationClipSettings(animationClip, srcClipInfo);
                        foreach (var n in AnimationUtility.GetCurveBindings(_baseAnimationClip))
                        {
                            var curve = AnimationUtility.GetEditorCurve(_baseAnimationClip, n);
                            animationClip.SetCurve(
                                relativePath: n.path,
                                type: n.type,
                                propertyName: n.propertyName,
                                curve: curve
                            );
                        }

                        AssetDatabase.AddObjectToAsset(animationClip, _controller);
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_controller));
                        AssetDatabase.Refresh();
                    }
                }
            }
            else if (clipList.Exists(item => item.name == _clipName) || string.IsNullOrEmpty(_clipName))
            {
                EditorGUILayout.LabelField("can't create duplicate names or empty");
            }
            else
            {
                var clipName = _clipName;
                if (GUILayout.Button("create"))
                {
                    var animationClip = UnityEditor.Animations.AnimatorController.AllocateAnimatorClip(clipName);
                    var path = AssetDatabase.GetAssetPath(_controller);
                    AssetDatabase.AddObjectToAsset(animationClip, _controller);
                    AssetDatabase.ImportAsset(path);
                    Debug.Log($"clip:{AssetDatabase.GetAssetPath(animationClip)}, path:{path}");
                    AssetDatabase.Refresh();
                }
            }

            EditorGUILayout.EndVertical();

            if (clipList.Count == 0)
                return;

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("remove clip");
            EditorGUILayout.BeginVertical("box");

            foreach (var removeClip in clipList)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(removeClip.name);
                if (GUILayout.Button("remove", GUILayout.Width(100)))
                {
                    DestroyImmediate(removeClip, true);
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_controller));
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }
    }
}