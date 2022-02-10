#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Wing.uPainter
{
    public class EditorUtils
    {
        public static bool Foldout(bool foldout, string content, Action ongui)
        {
            var style = new GUIStyle("ShurikenModuleTitle");
            style.font = new GUIStyle(EditorStyles.label).font;
            style.border = new RectOffset(1, 7, 4, 4);
            style.fixedHeight = 28;
            style.contentOffset = new Vector2(20f, -2f);

            var rect = GUILayoutUtility.GetRect(16f, 22f, style);
            GUI.Box(rect, content, style);

            var e = Event.current;

            var toggleRect = new Rect(rect.x + 4f, rect.y + 5f, 13f, 13f);
            if (e.type == EventType.Repaint)
            {
                EditorStyles.foldout.Draw(toggleRect, false, false, foldout, false);
            }

            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
            {
                foldout = !foldout;
                e.Use();
            }

            EditorGUI.indentLevel++;
            if (foldout && ongui != null)
            {
                ongui();
            }

            EditorGUI.indentLevel--;

            return foldout;
        }

        public static void ShowList(SerializedProperty list, bool showListSize = true, bool showListLabel = true, string label = "", bool showChildren = false, string childLabel = "")
        {
            if (showListLabel)
            {
                if (string.IsNullOrEmpty(label))
                {
                    EditorGUILayout.PropertyField(list);
                }
                else
                {
                    EditorGUILayout.PropertyField(list, new GUIContent(label));
                }
                EditorGUI.indentLevel += 1;
            }
            if (!showListLabel || list.isExpanded)
            {
                if (showListSize)
                {
                    EditorGUILayout.PropertyField(list.FindPropertyRelative("Array.size"));
                }
                for (int i = 0; i < list.arraySize; i++)
                {
                    if (string.IsNullOrEmpty(childLabel))
                    {
                        EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i), showListLabel);
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i), new GUIContent(childLabel), showListLabel);
                    }
                }
            }
            if (showListLabel)
            {
                EditorGUI.indentLevel -= 1;
            }
        }

        public static object GetParent(SerializedProperty prop)
        {
            if(prop == null)
            {
                return null;
            }
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements.Take(elements.Length - 1))
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue(obj, elementName, index);
                }
                else
                {
                    obj = GetValue(obj, element);
                }
            }
            return obj;
        }

        public static object GetValue(object source, string name)
        {
            if (source == null)
                return null;
            var type = source.GetType();
            var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (f == null)
            {
                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p == null)
                    return null;
                return p.GetValue(source, null);
            }
            return f.GetValue(source);
        }

        public static object GetValue(object source, string name, int index)
        {
            var enumerable = GetValue(source, name) as IEnumerable;
            var enm = enumerable.GetEnumerator();
            while (index-- >= 0)
                enm.MoveNext();
            return enm.Current;
        }
    }
}
#endif
