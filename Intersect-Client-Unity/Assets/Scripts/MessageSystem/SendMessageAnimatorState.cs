using UnityEngine;

namespace Intersect.Client.MessageSystem {
	public class SendMessageAnimatorState : StateMachineBehaviour {
		[SerializeField]
		private MessageTypes onStart = default;
		[SerializeField]
		private bool sendNowOnStart = false;
		[SerializeField]
		private MessageTypes onExit = default;
		[SerializeField]
		private bool sendNowOnExit = false;
		// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
		override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
			if (onStart != MessageTypes.None) {
				MessageManager.SendMessage(onStart, null, sendNowOnStart);
			}
		}

		// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
		//override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		//
		//}

		// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
		override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
			if (onExit != MessageTypes.None) {
				MessageManager.SendMessage(onExit, null, sendNowOnExit);
			}
		}

		// OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
		//override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		//
		//}

		// OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
		//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		//
		//}
	}
}
