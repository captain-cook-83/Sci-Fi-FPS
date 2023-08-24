#pragma warning disable 649
using UnityEngine;
using System.Linq;

namespace Pathfinding {
	/// <summary>
	/// Moves the target in example scenes.
	/// This is a simple script which has the sole purpose
	/// of moving the target point of agents in the example
	/// scenes for the A* Pathfinding Project.
	///
	/// It is not meant to be pretty, but it does the job.
	/// </summary>
	[HelpURL("http://arongranberg.com/astar/documentation/beta/class_pathfinding_1_1_target_mover.php")]
	public class TargetMover : VersionedMonoBehaviour {
		/// <summary>Mask for the raycast placement</summary>
		public LayerMask mask;

		public Transform target;

		/// <summary>Determines if the target position should be updated every frame or only on double-click</summary>
		bool onlyOnDoubleClick;
		public Trigger trigger;
		public GameObject clickEffect;
		public bool use2D;

		Camera cam;

		public enum Trigger {
			Continuously,
			SingleClick,
			DoubleClick
		}

		public void Start () {
			//Cache the Main Camera
			cam = Camera.main;
			useGUILayout = false;
		}

		public void OnGUI () {
			if (trigger != Trigger.Continuously && cam != null && Event.current.type == EventType.MouseDown) {
				if (Event.current.clickCount == (trigger == Trigger.DoubleClick ? 2 : 1)) {
					UpdateTargetPosition();
				}
			}
		}

		/// <summary>Update is called once per frame</summary>
		void Update () {
			if (trigger == Trigger.Continuously && cam != null) {
				UpdateTargetPosition();
			}
		}

		public void UpdateTargetPosition () {
			Vector3 newPosition = Vector3.zero;
			bool positionFound = false;
			Transform hitObject = null;

			if (use2D) {
				newPosition = cam.ScreenToWorldPoint(Input.mousePosition);
				newPosition.z = 0;
				positionFound = true;
			} else {
				// Fire a ray through the scene at the mouse position and place the target where it hits
				RaycastHit hit;
				if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, mask)) {
					newPosition = hit.point;
					hitObject = hit.transform;
					positionFound = true;
				}
			}

			if (positionFound && newPosition != target.position) {
				target.position = newPosition;

				if (trigger != Trigger.Continuously) {
					// Slightly inefficient way of finding all AIs, but this is just an example script, so it doesn't matter much.
					// FindObjectsOfType does not support interfaces unfortunately.
					IAstarAI[] ais = FindObjectsOfType<MonoBehaviour>().OfType<IAstarAI>().ToArray();

					if (hitObject != null && hitObject.TryGetComponent<Pathfinding.Examples.Interactable>(out var interactable)) {
						for (int i = 0; i < ais.Length; i++) {
							interactable.Interact(ais[i]);
						}
					} else {
						if (clickEffect != null && clickEffect != null) {
							GameObject.Instantiate(clickEffect, newPosition, Quaternion.identity);
						}

						for (int i = 0; i < ais.Length; i++) {
#if MODULE_ENTITIES
							var isFollowerEntity = ais[i] is FollowerEntity;
#else
							var isFollowerEntity = false;
#endif
							if (ais[i] != null) ais[i].destination = newPosition;

							// Make the agents recalculate their path immediately for slighly increased responsiveness
							if (ais[i] != null && !isFollowerEntity) ais[i].SearchPath();
						}
					}
				}
			}
		}

		protected override int OnUpgradeSerializedData (int version, bool unityThread) {
			if (version < 2) {
				trigger = onlyOnDoubleClick ? Trigger.DoubleClick : Trigger.Continuously;
			}
			return 2;
		}
	}
}
