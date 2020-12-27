using Android.Animation;

namespace Escorp.Android.Views
{
    internal class AnimatorUpdateListener : Java.Lang.Object, ValueAnimator.IAnimatorUpdateListener
    {
        private readonly Odometer view;

        public AnimatorUpdateListener(Odometer view) => this.view = view;

        public void OnAnimationUpdate(ValueAnimator animation)
        {
            view.columnManager.AnimationProgress = animation.AnimatedFraction;
            view.CheckForRelayout();
            view.Invalidate();
        }
    }
}