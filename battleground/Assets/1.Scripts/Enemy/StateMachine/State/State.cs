using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Action, Transition을 갖게되고 AI동작들을 호출시켜 준다.
/// State를 각각 만들어서 정의하게 된다.
/// </summary>
[CreateAssetMenu(menuName = "PluggableAI/State")]
public class State : ScriptableObject
{
    public Action[] actions;
    public Transition[] transitions;
    public Color sceneGizmoColor = Color.gray;

    public void DoActions(StateController controller)
    {
        for(int i = 0; i < actions.Length; i++)
        {
            actions[i].Act(controller);
        }
    }

    //State가 바뀔 때 사용
    public void OnEnableActions(StateController controller)
    {
        for (int i = 0; i < actions.Length; i++)
        {
            actions[i].OnReadyAction(controller); //초기에 초기화하는 함수들
        }
        for (int i = transitions.Length - 1; i >= 0; i--)
        {
            transitions[i].decision.OnEnableDecision(controller);
        }
    }

    public void CheckTransitions(StateController controller)
    {
        for (int i = 0; i < transitions.Length; i++)
        {
            bool decision = transitions[i].decision.Decide(controller);
            if (decision) //decision이 존재하면 true조건이 만족된것 그러므로 State가 바뀐다.
            {
                controller.TransitionToState(transitions[i].trueState, transitions[i].decision);
            }
            else //안바뀐다
            {
                controller.TransitionToState(transitions[i].falseState, transitions[i].decision);
            }
            if (controller.currentState != this) //State가 바뀐것이므로,
            {
                controller.currentState.OnEnableActions(controller);
                break;
            }
        }
    }
}
