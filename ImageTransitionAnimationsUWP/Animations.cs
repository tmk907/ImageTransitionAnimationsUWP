using System;
using System.Numerics;
using Windows.UI.Composition;

namespace ImageTransitionAnimationsUWP
{
    public class Animations
    {
        private readonly Compositor compositor;

        public Animations(Compositor compositor)
        {
            this.compositor = compositor;
        }

        public CompositionAnimation CreateScaleAnimation(Vector3 initialValue, Vector3 finalValue, TimeSpan duration)
        {
            var animation = compositor.CreateVector3KeyFrameAnimation();
            animation.InsertKeyFrame(0f, initialValue);
            animation.InsertKeyFrame(1f, finalValue);
            animation.Duration = duration;
            animation.Direction = AnimationDirection.Normal;
            animation.IterationCount = 1;
            animation.Target = "Scale";
            return animation;
        }

        public CompositionAnimation CreateScaleSpringAnimation(Vector3 initialValue, Vector3 finalValue, TimeSpan duration)
        {
            var animation = compositor.CreateSpringVector3Animation();
            animation.InitialValue = initialValue;
            animation.FinalValue = finalValue;
            animation.Period = duration;
            animation.StopBehavior = AnimationStopBehavior.SetToFinalValue;
            animation.Target = "Scale";
            return animation;
        }

        public CompositionAnimation CreateOpacityAnimation(float startValue, float endValue, TimeSpan duration)
        {
            var animation = compositor.CreateScalarKeyFrameAnimation();
            animation.InsertKeyFrame(0f, startValue);
            animation.InsertKeyFrame(1f, endValue);
            animation.Duration = duration;
            animation.Direction = AnimationDirection.Normal;
            animation.IterationCount = 1;
            animation.Target = "Opacity";
            return animation;
        }

        public CompositionAnimation CreateOpacitySpringAnimation(float startValue, float endValue, TimeSpan duration)
        {
            var animation = compositor.CreateSpringScalarAnimation();
            animation.InitialValue = startValue;
            //animation.StopBehavior = AnimationStopBehavior.SetToFinalValue;
            animation.Period = duration;
            animation.FinalValue = endValue;
            return animation;
        }

        public CompositionAnimation CreateSlideAnimation(Vector3 initialValue, Vector3 finalValue, TimeSpan duration)
        {
            var animation = compositor.CreateVector3KeyFrameAnimation();
            animation.InsertKeyFrame(0f, initialValue);
            animation.InsertKeyFrame(1f, finalValue);
            animation.Duration = duration;
            animation.Direction = AnimationDirection.Normal;
            animation.IterationCount = 1;
            animation.Target = "Offset";
            return animation;
        }

        public CompositionAnimation CreateSlideSpringAnimation(float startOffset, float endOffset, TimeSpan duration)
        {
            var animation = compositor.CreateSpringVector3Animation();
            animation.InitialValue = new Vector3(startOffset, 0f, 0f);
            animation.FinalValue = new Vector3(endOffset, 0f, 0f);
            animation.StopBehavior = AnimationStopBehavior.SetToFinalValue;
            animation.Period = duration;
            return animation;
        }
    }
}
