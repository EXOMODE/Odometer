using Android.Animation;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Runtime;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Views.Animations;

namespace Escorp.Android.Views
{
    [Register("Escorp.Android.Views.Odometer")]
    public class Odometer : View
    {
        private readonly bool InstanceFieldsInitialized = false;

        private void InitializeInstanceFields()
        {
            metrics = new DrawMetrics(textPaint);
            columnManager = new ColumnManager(metrics);
        }

        internal const int DefaultTextSize = 12;
        internal static readonly Color DefaultTextColor = Color.Black;
        private const int DefaultAnimationDuration = 350;
        private static readonly IInterpolator DefaultAnimationInterpolator = new AccelerateDecelerateInterpolator();
        internal static readonly GravityFlags DefaultGravity = GravityFlags.Start;

        protected internal readonly Paint textPaint = new TextPaint(PaintFlags.AntiAlias);

        internal DrawMetrics metrics;
        internal ColumnManager columnManager;

        private readonly ValueAnimator animator = ValueAnimator.OfFloat(1f);
        private readonly Rect viewBounds = new Rect();

        private string text;

        private int lastMeasuredDesiredWidth, lastMeasuredDesiredHeight;

        private GravityFlags gravity;
        private Color textColor;
        private float textSize;
        private TypefaceStyle textStyle;
        private long animationDelayInMillis;
        private long animationDurationInMillis;
        private IInterpolator animationInterpolator;
        private bool isAnimateMeasurementChanged;
        private string pendingTextToSet;

        public Odometer(Context context, IAttributeSet attrs, int defStyle, int defStyleRes) : base(context, attrs, defStyle, defStyleRes)
        {
            if (!InstanceFieldsInitialized)
            {
                InitializeInstanceFields();
                InstanceFieldsInitialized = true;
            }

            Initialize(context, attrs, defStyle, defStyleRes);
        }

        public Odometer(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle, 0)
        {
            if (!InstanceFieldsInitialized)
            {
                InitializeInstanceFields();
                InstanceFieldsInitialized = true;
            }

            Initialize(context, attrs, defStyle, 0);
        }

        public Odometer(Context context, IAttributeSet attrs) : base(context, attrs, 0, 0)
        {
            if (!InstanceFieldsInitialized)
            {
                InitializeInstanceFields();
                InstanceFieldsInitialized = true;
            }

            Initialize(context, attrs, 0, 0);
        }

        public Odometer(Context context) : base(context, null, 0, 0)
        {
            if (!InstanceFieldsInitialized)
            {
                InitializeInstanceFields();
                InstanceFieldsInitialized = true;
            }

            Initialize(context, null, 0, 0);
        }

        public Odometer(System.IntPtr javaRef, JniHandleOwnership transfer) : base(javaRef, transfer)
        {
            if (!InstanceFieldsInitialized)
            {
                InitializeInstanceFields();
                InstanceFieldsInitialized = true;
            }

            Initialize(Application.Context, null, 0, 0);
        }

        protected virtual void Initialize(Context context, IAttributeSet attrs, int defStyle, int defStyleRes)
        {
            if (context == null) return;

            OdometerStyledAttributes styledAttributes = new OdometerStyledAttributes(this, context.Resources);
            TypedArray arr = context.ObtainStyledAttributes(attrs, Resource.Styleable.odometer, defStyle, defStyleRes);
            int textAppearanceResId = arr.GetResourceId(Resource.Styleable.odometer_android_textAppearance, -1);

            if (textAppearanceResId != -1)
            {
                TypedArray textAppearanceArr = context.ObtainStyledAttributes(textAppearanceResId, Resource.Styleable.odometer);
                styledAttributes.ApplyTypedArray(textAppearanceArr);
                textAppearanceArr.Recycle();
            }

            styledAttributes.ApplyTypedArray(arr);

            animationInterpolator = DefaultAnimationInterpolator;
            animationDurationInMillis = arr.GetInt(Resource.Styleable.odometer_animationDuration, DefaultAnimationDuration);
            isAnimateMeasurementChanged = arr.GetBoolean(Resource.Styleable.odometer_animateMeasurementChange, false);
            gravity = styledAttributes.Gravity;

            if (styledAttributes.ShadowColor != 0) textPaint.SetShadowLayer(styledAttributes.ShadowRadius, styledAttributes.ShadowDx, styledAttributes.ShadowDy, styledAttributes.ShadowColor);

            if (styledAttributes.TextStyle != 0)
            {
                textStyle = styledAttributes.TextStyle;
                Typeface = textPaint.Typeface;
            }

            TextColor = styledAttributes.TextColor;
            TextSize = styledAttributes.TextSize;

            int defaultCharList = arr.GetInt(Resource.Styleable.odometer_defaultCharacterList, 0);

            switch (defaultCharList)
            {
                case 1:
                    SetCharacterLists(Utils.ProvideNumberList);
                    break;

                case 2:
                    SetCharacterLists(Utils.ProvideAlphabeticalList);
                    break;

                default:
                    SetCharacterLists(Utils.ProvideNumberList, Utils.ProvideAlphabeticalList);
                    break;
            }

            int defaultPreferredScrollingDirection = arr.GetInt(Resource.Styleable.odometer_defaultPreferredScrollingDirection, 0);

            metrics.PreferredScrollingDirection = defaultPreferredScrollingDirection switch
            {
                0 => ScrollingDirection.Any,
                1 => ScrollingDirection.Up,
                2 => ScrollingDirection.Down,
                _ => throw new System.ArgumentException("Unsupported ticker_defaultPreferredScrollingDirection: " + defaultPreferredScrollingDirection),
            };

            if (IsCharacterListsSet)
                SetText(styledAttributes.Text, false);
            else
                pendingTextToSet = styledAttributes.Text;

            arr.Recycle();

            animator.AddUpdateListener(new AnimatorUpdateListener(this));
            animator.AddListener(new AnimationAdapter(this));
        }

        public virtual string[] CharacterLists
        {
            set
            {
                columnManager.SetCharacterLists(value);

                if (pendingTextToSet != null)
                {
                    SetText(pendingTextToSet, false);
                    pendingTextToSet = null;
                }
            }
        }

        public virtual void SetText(string text, bool animate)
        {
            if (TextUtils.Equals(text, this.text)) return;
            if (animator.IsRunning) animator.Cancel();

            this.text = text;
            char[] targetText = text is null ? new char[0] : text.ToCharArray();

            columnManager.Text = targetText;
            ContentDescription = text;

            if (animate)
            {
                animator.StartDelay = animationDelayInMillis;
                animator.SetDuration(animationDurationInMillis);
                animator.SetInterpolator(animationInterpolator);
                animator.Start();
            }
            else
            {
                columnManager.AnimationProgress = 1f;
                columnManager.OnAnimationEnd();
                CheckForRelayout();
                Invalidate();
            }
        }

        //public override void SetText(ICharSequence text, BufferType type) => Text = text.ToString();

        public virtual string Text
        {
            get => text;
            set => SetText(value, !TextUtils.IsEmpty(value));
        }

        public virtual Color TextColor
        {
            get => textColor;

            set
            {
                if (textColor != value)
                {
                    textColor = value;
                    textPaint.Color = textColor;
                    Invalidate();
                }
            }
        }

        public virtual bool IsCharacterListsSet => columnManager.GetCharacterLists() != null;

        public virtual float TextSize
        {
            get => textSize;

            set
            {
                if (textSize != value)
                {
                    textSize = value;
                    textPaint.TextSize = value;
                    OnTextPaintMeasurementChanged();
                }
            }
        }

        public virtual Typeface Typeface
        {
            get => textPaint.Typeface;

            set
            {
                if (textStyle == TypefaceStyle.BoldItalic)
                    value = Typeface.Create(value, TypefaceStyle.BoldItalic);
                else if (textStyle == TypefaceStyle.Bold)
                    value = Typeface.Create(value, TypefaceStyle.Bold);
                else if (textStyle == TypefaceStyle.Italic)
                    value = Typeface.Create(value, TypefaceStyle.Italic);

                textPaint.SetTypeface(value);
                OnTextPaintMeasurementChanged();
            }
        }

        public virtual long AnimationDelay
        {
            get => animationDelayInMillis;
            set => animationDelayInMillis = value;
        }

        public virtual long AnimationDuration
        {
            get => animationDurationInMillis;
            set => animationDurationInMillis = value;
        }

        public virtual IInterpolator AnimationInterpolator
        {
            get => animationInterpolator;
            set => animationInterpolator = value;
        }

        public virtual ScrollingDirection PreferredScrollingDirection
        {
            set => metrics.PreferredScrollingDirection = value;
        }

        public virtual GravityFlags Gravity
        {
            get => gravity;

            set
            {
                if (gravity != value)
                {
                    gravity = value;
                    Invalidate();
                }
            }
        }

        public virtual bool IsAnimateMeasurementChanged
        {
            set => isAnimateMeasurementChanged = value;
            get => isAnimateMeasurementChanged;
        }

        protected virtual void OnTextPaintMeasurementChanged()
        {
            metrics.Invalidate();
            CheckForRelayout();
            Invalidate();
        }

        public void CheckForRelayout()
        {
            bool widthChanged = lastMeasuredDesiredWidth != ComputeDesiredWidth();
            bool heightChanged = lastMeasuredDesiredHeight != ComputeDesiredHeight();

            if (widthChanged || heightChanged) RequestLayout();
        }

        protected int ComputeDesiredWidth()
        {
            int contentWidth = (int)(IsAnimateMeasurementChanged ? columnManager.CurrentWidth : columnManager.MinimumRequiredWidth);
            return contentWidth + PaddingLeft + PaddingRight;
        }

        protected int ComputeDesiredHeight() => (int)metrics.CharHeight + PaddingTop + PaddingBottom;

        protected void RealignAndClipCanvasForGravity(Canvas canvas)
        {
            float currentWidth = columnManager.CurrentWidth;
            float currentHeight = metrics.CharHeight;
            RealignAndClipCanvasForGravity(canvas, gravity, viewBounds, currentWidth, currentHeight);
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            lastMeasuredDesiredWidth = ComputeDesiredWidth();
            lastMeasuredDesiredHeight = ComputeDesiredHeight();

            int desiredWidth = ResolveSize(lastMeasuredDesiredWidth, widthMeasureSpec);
            int desiredHeight = ResolveSize(lastMeasuredDesiredHeight, heightMeasureSpec);

            SetMeasuredDimension(desiredWidth, desiredHeight);
        }

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);
            viewBounds.Set(PaddingLeft, PaddingTop, w - PaddingRight, h - PaddingBottom);
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);

            canvas.Save();

            RealignAndClipCanvasForGravity(canvas);

            canvas.Translate(0f, metrics.CharBaseline);

            columnManager.Draw(canvas, textPaint);

            canvas.Restore();
        }

        public void SetCharacterLists(params string[] characterLists)
        {
            columnManager.SetCharacterLists(characterLists);

            if (pendingTextToSet != null)
            {
                SetText(pendingTextToSet, false);
                pendingTextToSet = null;
            }
        }

        public static void RealignAndClipCanvasForGravity(Canvas canvas, GravityFlags gravity, Rect viewBounds, float currentWidth, float currentHeight)
        {
            int availableWidth = viewBounds.Width();
            int availableHeight = viewBounds.Height();

            float translationX = 0;
            float translationY = 0;

            if ((gravity & GravityFlags.CenterVertical) == GravityFlags.CenterVertical) translationY = viewBounds.Top + (availableHeight - currentHeight) / 2f;
            if ((gravity & GravityFlags.CenterHorizontal) == GravityFlags.CenterHorizontal) translationX = viewBounds.Left + (availableWidth - currentWidth) / 2f;
            if ((gravity & GravityFlags.Top) == GravityFlags.Top) translationY = 0;
            if ((gravity & GravityFlags.Bottom) == GravityFlags.Bottom) translationY = viewBounds.Top + (availableHeight - currentHeight);
            if ((gravity & GravityFlags.Start) == GravityFlags.Start) translationX = 0;
            if ((gravity & GravityFlags.End) == GravityFlags.End) translationX = viewBounds.Left + (availableWidth - currentWidth);

            canvas.Translate(translationX, translationY);
            canvas.ClipRect(0f, 0f, currentWidth, currentHeight);
        }
    }
}