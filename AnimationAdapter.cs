using Android.Animation;

namespace Escorp.Android.Views
{
    internal class AnimationAdapter : AnimatorListenerAdapter
    {
        private readonly Odometer view;

        public AnimationAdapter(Odometer view) => this.view = view;

        public override void OnAnimationEnd(Animator animation)
        {
            view.columnManager.OnAnimationEnd();
            view.CheckForRelayout();
            view.Invalidate();
        }
    }
}