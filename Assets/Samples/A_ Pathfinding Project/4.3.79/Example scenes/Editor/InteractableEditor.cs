using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Pathfinding.Examples;
using System.Collections.Generic;

namespace Pathfinding.Examples {
	[CustomEditor(typeof(Interactable))]
	[CanEditMultipleObjects]
	public class InteractableEditor : EditorBase {
		ReorderableList actions;

		static Rect SliceRow (ref Rect rect, float height) {
			var r = new Rect(rect.x, rect.y, rect.width, height);
			rect.yMin += height + EditorGUIUtility.standardVerticalSpacing;
			return r;
		}
		protected override void OnEnable () {
			base.OnEnable();
			actions = new ReorderableList(serializedObject, serializedObject.FindProperty("actions"), true, true, true, true);
			actions.drawElementCallback = (Rect rect, int index, bool active, bool isFocused) => {
				var item = actions.serializedProperty.GetArrayElementAtIndex(index);
				var ob = item.managedReferenceValue as Interactable.InteractableAction;
				if (ob == null) {
					EditorGUI.LabelField(rect, "Null");
					return;
				}
				var tp = ob.GetType();

				var lineHeight = EditorGUIUtility.singleLineHeight;
				if (tp == typeof(Interactable.AnimatorSetBoolAction)) {
					EditorGUI.LabelField(SliceRow(ref rect, lineHeight), "Set Animator Property", EditorStyles.boldLabel);
					EditorGUI.PropertyField(SliceRow(ref rect, lineHeight), item.FindPropertyRelative("animator"));
					EditorGUI.PropertyField(SliceRow(ref rect, lineHeight), item.FindPropertyRelative("propertyName"));
					EditorGUI.PropertyField(SliceRow(ref rect, lineHeight), item.FindPropertyRelative("value"));
					EditorGUI.PropertyField(SliceRow(ref rect, lineHeight), item.FindPropertyRelative("waitUntilFinished"));
				} else if (tp == typeof(Interactable.ActivateParticleSystem)) {
					EditorGUI.LabelField(SliceRow(ref rect, lineHeight), "Activate Particle System", EditorStyles.boldLabel);
					EditorGUI.PropertyField(SliceRow(ref rect, lineHeight), item.FindPropertyRelative("particleSystem"));
				} else if (tp == typeof(Interactable.DelayAction)) {
					EditorGUI.LabelField(SliceRow(ref rect, lineHeight), "Delay", EditorStyles.boldLabel);
					EditorGUI.PropertyField(SliceRow(ref rect, lineHeight), item.FindPropertyRelative("delay"));
				} else if (tp == typeof(Interactable.SetObjectActiveAction)) {
					EditorGUI.LabelField(SliceRow(ref rect, lineHeight), "Set Object Active", EditorStyles.boldLabel);
					EditorGUI.PropertyField(SliceRow(ref rect, lineHeight), item.FindPropertyRelative("target"));
					EditorGUI.PropertyField(SliceRow(ref rect, lineHeight), item.FindPropertyRelative("active"));
				} else if (tp == typeof(Interactable.TeleportAgentAction)) {
					EditorGUI.LabelField(SliceRow(ref rect, lineHeight), "Teleport Agent", EditorStyles.boldLabel);
					EditorGUI.PropertyField(SliceRow(ref rect, lineHeight), item.FindPropertyRelative("destination"));
				} else if (tp == typeof(Interactable.MoveToAction)) {
					EditorGUI.LabelField(SliceRow(ref rect, lineHeight), "Move To", EditorStyles.boldLabel);
					EditorGUI.PropertyField(SliceRow(ref rect, lineHeight), item.FindPropertyRelative("destination"));
					EditorGUI.PropertyField(SliceRow(ref rect, lineHeight), item.FindPropertyRelative("useRotation"));
					EditorGUI.PropertyField(SliceRow(ref rect, lineHeight), item.FindPropertyRelative("waitUntilReached"));
				} else if (tp == typeof(Interactable.InstantiatePrefab)) {
					EditorGUI.LabelField(SliceRow(ref rect, lineHeight), "Instantiate Prefab", EditorStyles.boldLabel);
					EditorGUI.PropertyField(SliceRow(ref rect, lineHeight), item.FindPropertyRelative("prefab"));
					EditorGUI.PropertyField(SliceRow(ref rect, lineHeight), item.FindPropertyRelative("position"));
				} else if (tp == typeof(Interactable.CallFunction)) {
					EditorGUI.LabelField(SliceRow(ref rect, lineHeight), "Call Function", EditorStyles.boldLabel);
					EditorGUI.PropertyField(SliceRow(ref rect, lineHeight), item.FindPropertyRelative("function"));
				}
			};
			actions.elementHeightCallback = (int index) => {
				var actions = (target as Interactable).actions;
				var tp = index < actions.Count ? actions[index]?.GetType() : null;
				var h = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
				if (tp == null) return h;
				else if (tp == typeof(Interactable.AnimatorSetBoolAction)) return 5*h;
				else if (tp == typeof(Interactable.ActivateParticleSystem)) return 2*h;
				else if (tp == typeof(Interactable.DelayAction)) return 2*h;
				else if (tp == typeof(Interactable.SetObjectActiveAction)) return 3*h;
				else if (tp == typeof(Interactable.TeleportAgentAction)) return 2*h;
				else if (tp == typeof(Interactable.MoveToAction)) return 4*h;
				else if (tp == typeof(Interactable.InstantiatePrefab)) return 3*h;
				else if (tp == typeof(Interactable.CallFunction)) {
					return (3.5f + Mathf.Max(1, (actions[index] as Interactable.CallFunction).function.GetPersistentEventCount())*2.5f) * h;
				} else throw new System.Exception("Unexpected type " + tp);
			};
			actions.drawHeaderCallback = (Rect rect) => {
				EditorGUI.LabelField(rect, "Actions");
			};
			actions.onAddDropdownCallback = (rect, list) => {
				GenericMenu menu = new GenericMenu();
				menu.AddItem(new GUIContent("AnimationAction"), false, () => (target as Interactable).actions.Add(new Interactable.AnimatorSetBoolAction()));
				menu.AddItem(new GUIContent("ActivateParticleSystem"), false, () => (target as Interactable).actions.Add(new Interactable.ActivateParticleSystem()));
				menu.AddItem(new GUIContent("DelayAction"), false, () => (target as Interactable).actions.Add(new Interactable.DelayAction()));
				menu.AddItem(new GUIContent("SetObjectActiveAction"), false, () => (target as Interactable).actions.Add(new Interactable.SetObjectActiveAction()));
				menu.AddItem(new GUIContent("TeleportAgentAction"), false, () => (target as Interactable).actions.Add(new Interactable.TeleportAgentAction()));
				menu.AddItem(new GUIContent("MoveToAction"), false, () => (target as Interactable).actions.Add(new Interactable.MoveToAction()));
				menu.AddItem(new GUIContent("InstantiatePrefab"), false, () => (target as Interactable).actions.Add(new Interactable.InstantiatePrefab()));
				menu.AddItem(new GUIContent("CallFunction"), false, () => (target as Interactable).actions.Add(new Interactable.CallFunction()));
				menu.DropDown(rect);
			};
		}

		protected override void Inspector () {
			var script = target as Interactable;

			script.actions = script.actions ?? new List<Interactable.InteractableAction>();
			actions.DoLayoutList();
		}
	}
}
