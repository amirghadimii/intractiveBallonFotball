using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorUtil : MonoBehaviour {

    public static Animator SetupAnimator(Animator animator, GameObject parent)
    {
        var _animator = animator;

        foreach(var childAnimator in parent.GetComponentsInChildren<Animator>())
        {
            if (childAnimator != _animator)
            {
                _animator.avatar = childAnimator.avatar;
                Destroy(childAnimator);
                break;
            }
        }

        return _animator;
    }

}
