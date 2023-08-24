using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding.Examples {
	public enum CoroutineAction {
		Tick,
		Cancel,
	}

	/// <summary>
	/// Example script for handling interactable objects in the example scenes.
	///
	/// It implements a very simple and lightweight state machine.
	///
	/// Note: This is an example script intended for the A* Pathfinding Project's example scenes.
	/// If you need a proper state machine for your game, you may be better served by other state machine solutions on the Unity Asset Store.
	///
	/// It works by keeping a linear list of states, each with an associated action.
	/// When an agent iteracts with this object, it immediately does the first action in the list.
	/// Once that action is done, it will do the next action and so on.
	///
	/// Some actions may cancel the whole interaction. For example the MoveTo action will cancel the interaction if the agent
	/// suddenly had its destination to something else. Presumably because the agent was interrupted by something.
	/// </summary>
	public class Interactable : VersionedMonoBehaviour {
		[System.Serializable]
		public abstract class InteractableAction {
			public abstract IEnumerator<CoroutineAction> Execute(IAstarAI ai);
		}

		[System.Serializable]
		public class AnimatorSetBoolAction : InteractableAction {
			public string propertyName;
			public bool value;
			public Animator animator;
			public bool waitUntilFinished;

			public override IEnumerator<CoroutineAction> Execute (IAstarAI ai) {
				// var stateHash = Animator.StringToHash(animationName);
				animator.SetBool(propertyName, value);
				yield return CoroutineAction.Tick;
				Debug.Log(animator.GetNextAnimatorStateInfo(0).IsName("Door_Open"));
				Debug.Log(animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") + " " + animator.GetCurrentAnimatorStateInfo(0).IsName("Door_Open"));
				yield break;

				// if (waitUntilFinished) {
				//     // Wait until the clip starts
				//     while (animator.GetCurrentAnimatorStateInfo(0).fullPathHash != stateHash) {
				//         yield return CoroutineAction.Tick;
				//     }

				//     // Wait while playing the clip
				//     while (animator.GetCurrentAnimatorStateInfo(0).fullPathHash == stateHash) {
				//         yield return CoroutineAction.Tick;
				//     }
				// }
			}
		}

		[System.Serializable]
		public class ActivateParticleSystem : InteractableAction {
			public ParticleSystem particleSystem;

			public override IEnumerator<CoroutineAction> Execute (IAstarAI ai) {
				particleSystem.Play();
				yield break;
			}
		}

		[System.Serializable]
		public class DelayAction : InteractableAction {
			public float delay;

			public override IEnumerator<CoroutineAction> Execute (IAstarAI ai) {
				float time = Time.time;
				while (Time.time < time) yield return CoroutineAction.Tick;
				yield break;
			}
		}

		[System.Serializable]
		public class SetObjectActiveAction : InteractableAction {
			public GameObject target;
			public bool active;

			public override IEnumerator<CoroutineAction> Execute (IAstarAI ai) {
				target.SetActive(active);
				yield break;
			}
		}

		[System.Serializable]
		public class InstantiatePrefab : InteractableAction {
			public GameObject prefab;
			public Transform position;

			public override IEnumerator<CoroutineAction> Execute (IAstarAI ai) {
				if (prefab != null && position != null) {
					GameObject.Instantiate(prefab, position.position, position.rotation);
				}
				yield break;
			}
		}

		[System.Serializable]
		public class CallFunction : InteractableAction {
			public UnityEngine.Events.UnityEvent function;

			public override IEnumerator<CoroutineAction> Execute (IAstarAI ai) {
				function.Invoke();
				yield break;
			}
		}

		[System.Serializable]
		public class TeleportAgentAction : InteractableAction {
			public Transform destination;

			public override IEnumerator<CoroutineAction> Execute (IAstarAI ai) {
				ai.Teleport(destination.position);
				yield break;
			}
		}

		[System.Serializable]
		public class MoveToAction : InteractableAction {
			public Transform destination;
			public bool useRotation;
			public bool waitUntilReached;

			public override IEnumerator<CoroutineAction> Execute (IAstarAI ai) {
				var dest = destination.position;
#if MODULE_ENTITIES
				if (useRotation && ai is FollowerEntity follower) {
					follower.SetDestination(dest, destination.rotation * Vector3.forward);
				} else
#endif
				{
					if (useRotation) Debug.LogError("useRotation is only supported for FollowerEntity agents", ai as MonoBehaviour);
					ai.destination = dest;
				}

				if (waitUntilReached) {
					while (!ai.reachedDestination) {
						if (ai.destination != dest) {
							// Something else must have changed the destination
							yield return CoroutineAction.Cancel;
						}
						if (ai.reachedEndOfPath) {
							// We have reached the end of the path, but not the destination
							// This must mean that we cannot get any closer
							// TODO: More accurate 'cannot move forwards' check
							yield return CoroutineAction.Cancel;
						}
						yield return CoroutineAction.Tick;
					}
				}
			}
		}

		[SerializeReference]
		public List<InteractableAction> actions;

		public void Interact (IAstarAI ai) {
			StartCoroutine(InteractCoroutine(ai));
		}

		IEnumerator InteractCoroutine (IAstarAI ai) {
			ai.destination = transform.position;

			if (actions.Count == 0) {
				Debug.LogWarning("No actions have been set up for this interactable", this);
				yield break;
			}

			var actionIndex = 0;
			while (actionIndex < actions.Count) {
				var action = actions[actionIndex];
				if (action == null) {
					actionIndex++;
					continue;
				}

				var enumerator = action.Execute(ai);
				while (enumerator.MoveNext()) {
					switch (enumerator.Current) {
					case CoroutineAction.Tick:
						yield return null;
						break;
					case CoroutineAction.Cancel:
						yield break;
					}
				}

				actionIndex++;
			}
		}
	}
}
