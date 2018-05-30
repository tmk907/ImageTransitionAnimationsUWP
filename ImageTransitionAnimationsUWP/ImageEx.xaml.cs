using System;
using System.Numerics;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ImageTransitionAnimationsUWP
{
    public sealed partial class ImageEx : UserControl
    {
        public ImageEx()
        {
            this.InitializeComponent();

            compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            visualB = ElementCompositionPreview.GetElementVisual(imageB);
            visualF = ElementCompositionPreview.GetElementVisual(imageF);
            visualC = ElementCompositionPreview.GetElementVisual(canvas);
            animations = new Animations(compositor);
            visualC.Clip = compositor.CreateInsetClip(0, 0, 0, 0);
            AnimationType = AnimationType.Opacity;
        }

        bool isFrontVisible = true;

        private Compositor compositor;
        private Visual visualB;
        private Visual visualF;
        private Visual visualC;
        private Animations animations;

        private readonly Vector3 vZero = new Vector3(0f, 0f, 0f);
        private readonly Vector3 vOne = new Vector3(1f, 1f, 1f);

        private int ImageWidth { get { return DecodePixelWidth; } }
        private int ImageHeight { get { return DecodePixelHeight; } }

        public AnimationType AnimationType
        {
            get { return (AnimationType)GetValue(AnimationTypeProperty); }
            set
            {
                SetValue(AnimationTypeProperty, value);
                OnAnimationTypeChanged(value);
            }
        }

        public static readonly DependencyProperty AnimationTypeProperty =
            DependencyProperty.Register("AnimationType", typeof(AnimationType), typeof(ImageEx), new PropertyMetadata(AnimationType.Opacity));

        private void OnAnimationTypeChanged(AnimationType value)
        {
            switch (value)
            {
                case AnimationType.Opacity:
                case AnimationType.OpacitySpring:
                case AnimationType.ScaleAndOpacity:
                    visualB.Offset = new Vector3(0f, 0f, 0f);
                    visualF.Offset = new Vector3(0f, 0f, 0f);
                    visualB.Opacity = isFrontVisible ? 0 : 1;
                    visualF.Opacity = isFrontVisible ? 1 : 0;
                    break;
                case AnimationType.SlideHorizontally:
                case AnimationType.SlideVertically:
                    visualF.Opacity = 1;
                    visualB.Opacity = 1;
                    break;
                case AnimationType.StackFromLeft:
                case AnimationType.StackFromRight:
                case AnimationType.StackFromTop:
                case AnimationType.StackFromBottom:
                    visualB.Offset = new Vector3(isFrontVisible ? ImageWidth : 0, 0f, 0f);
                    visualF.Offset = new Vector3(isFrontVisible ? 0 : ImageWidth, 0f, 0f);
                    visualF.Opacity = 1;
                    visualB.Opacity = 1;
                    break;
                default:

                    break;
            }
        }

        public Direction Direction
        {
            get { return (Direction)GetValue(DirectionProperty); }
            set { SetValue(DirectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Direction.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DirectionProperty =
            DependencyProperty.Register("Direction", typeof(Direction), typeof(ImageEx), new PropertyMetadata(Direction.Next));

        public int DecodePixelHeight
        {
            get { return (int)GetValue(DecodePixelHeightProperty); }
            set { SetValue(DecodePixelHeightProperty, value); }
        }

        public static readonly DependencyProperty DecodePixelHeightProperty =
            DependencyProperty.Register("DecodePixelHeight", typeof(int), typeof(int), null);

        public int DecodePixelWidth
        {
            get { return (int)GetValue(DecodePixelWidthProperty); }
            set { SetValue(DecodePixelWidthProperty, value); }
        }

        public static readonly DependencyProperty DecodePixelWidthProperty =
            DependencyProperty.Register("DecodePixelWidth", typeof(int), typeof(int), null);

        public Stretch Stretch
        {
            get { return (Stretch)GetValue(StretchProperty); }
            set
            {
                SetValue(StretchProperty, value);
                OnStretchChanged();
            }
        }

        private void OnStretchChanged()
        {
            imageF.Stretch = Stretch;
            imageB.Stretch = Stretch;
        }

        public static readonly DependencyProperty StretchProperty =
            DependencyProperty.Register("Stretch", typeof(Stretch), typeof(ImageEx), null);

        public TimeSpan Duration
        {
            get { return (TimeSpan)GetValue(DurationProperty); }
            set
            {
                SetValue(DurationProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for FadeOutDuration.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.Register("Duration", typeof(TimeSpan), typeof(ImageEx), new PropertyMetadata(TimeSpan.FromSeconds(1)));

        public Uri ImageUri
        {
            get { return (Uri)GetValue(ImageUriProperty); }
            set
            {
                SetValue(ImageUriProperty, value);
                OnImageUriChange();
            }
        }

        public static readonly DependencyProperty ImageUriProperty =
            DependencyProperty.Register("ImageUri", typeof(Uri), typeof(ImageEx), null);

        private void OnImageUriChange()
        {
            Animate();
        }

        private void Animate()
        {
            switch (AnimationType)
            {
                case AnimationType.Opacity:
                    OpacityAnimation();
                    break;
                case AnimationType.OpacitySpring:
                    OpacitySpringAnimation();
                    break;
                case AnimationType.ScaleAndOpacity:
                    ScaleAndOpacityAnimation();
                    break;
                case AnimationType.SlideHorizontally:
                    SlideAnimation(true);
                    break;
                case AnimationType.SlideVertically:
                    SlideAnimation(false);
                    break;
                case AnimationType.StackFromLeft:
                    StackHorizontalAnimation(true, true);
                    break;
                case AnimationType.StackFromRight:
                    StackHorizontalAnimation(true, false);
                    break;
                case AnimationType.StackFromTop:
                    StackHorizontalAnimation(false, true);
                    break;
                case AnimationType.StackFromBottom:
                    StackHorizontalAnimation(false, false);
                    break;
                case AnimationType.StackAndScaleFromLeft:
                    StackAndScaleAnimation(true, true);
                    break;
                case AnimationType.StackAndScaleFromRight:
                    StackAndScaleAnimation(true, false);
                    break;
                case AnimationType.StackAndScaleFromTop:
                    StackAndScaleAnimation(false, true);
                    break;
                case AnimationType.StackAndScaleFromBottom:
                    StackAndScaleAnimation(false, false);
                    break;
                default:
                    break;
            }

            isFrontVisible = !isFrontVisible;
        }

        private void OpacityAnimation()
        {
            if (isFrontVisible)
            {
                imageB.Source = GetImage(ImageUri);
                visualB.StartAnimation("Opacity", animations.CreateOpacityAnimation(0f, 1f, Duration));
                visualF.StartAnimation("Opacity", animations.CreateOpacityAnimation(1f, 0f, Duration));
            }
            else
            {
                imageF.Source = GetImage(ImageUri);
                visualB.StartAnimation("Opacity", animations.CreateOpacityAnimation(1f, 0f, Duration));
                visualF.StartAnimation("Opacity", animations.CreateOpacityAnimation(0f, 1f, Duration));
            }
        }

        private void OpacitySpringAnimation()
        {
            if (isFrontVisible)
            {
                imageB.Source = GetImage(ImageUri);
                visualB.StartAnimation("Opacity", animations.CreateOpacitySpringAnimation(0f, 1f, Duration));
                visualF.StartAnimation("Opacity", animations.CreateOpacitySpringAnimation(1f, 0f, Duration));
            }
            else
            {
                imageF.Source = GetImage(ImageUri);
                visualB.StartAnimation("Opacity", animations.CreateOpacitySpringAnimation(1f, 0f, Duration));
                visualF.StartAnimation("Opacity", animations.CreateOpacitySpringAnimation(0f, 1f, Duration));
            }
        }

        private void SlideAnimation(bool horizontally)
        {
            Vector3 leftTop;
            Vector3 rightBottom;
            if (horizontally)
            {
                leftTop = new Vector3(ImageWidth, 0f, 0f);
                rightBottom = new Vector3(-ImageWidth, 0f, 0f);
            }
            else
            {
                leftTop = new Vector3(0f, ImageHeight, 0f);
                rightBottom = new Vector3(0f, -ImageHeight, 0f);
            }
            if (isFrontVisible)
            {
                imageB.Source = GetImage(ImageUri);
            }
            else
            {
                imageF.Source = GetImage(ImageUri);
            }
            if (Direction == Direction.Next)
            {
                if (isFrontVisible)
                {
                    visualB.StartAnimation(nameof(visualB.Offset), animations.CreateSlideAnimation(leftTop, vZero, Duration));
                    visualF.StartAnimation(nameof(visualF.Offset), animations.CreateSlideAnimation(vZero, rightBottom, Duration));
                }
                else
                {
                    visualB.StartAnimation(nameof(visualB.Offset), animations.CreateSlideAnimation(vZero, rightBottom, Duration));
                    visualF.StartAnimation(nameof(visualF.Offset), animations.CreateSlideAnimation(leftTop, vZero, Duration));
                }
            }
            else
            {
                if (isFrontVisible)
                {
                    visualB.StartAnimation(nameof(visualB.Offset), animations.CreateSlideAnimation(rightBottom, vZero, Duration));
                    visualF.StartAnimation(nameof(visualF.Offset), animations.CreateSlideAnimation(vZero, leftTop, Duration));
                }
                else
                {
                    visualB.StartAnimation(nameof(visualB.Offset), animations.CreateSlideAnimation(vZero, leftTop, Duration));
                    visualF.StartAnimation(nameof(visualF.Offset), animations.CreateSlideAnimation(rightBottom, vZero, Duration));
                }
            }
        }

        private void StackHorizontalAnimation(bool horizontally, bool fromLeftTop)
        {
            int dir = fromLeftTop ? -1 : 1;
            if (isFrontVisible)
            {
                imageB.Source = GetImage(ImageUri);
                visualB.Offset = new Vector3(0f, 0f, 0f);
            }
            else
            {
                imageF.Source = GetImage(ImageUri);
                visualF.Offset = new Vector3(0f, 0f, 0f);
            }
            Vector3 vector;
            if (horizontally)
            {
                vector = new Vector3(dir * ImageWidth, 0f, 0f);
            }
            else
            {
                vector = new Vector3(0f, dir * ImageHeight, 0f);
            }
            if (Direction == Direction.Next)
            {
                if (isFrontVisible)
                {
                    imageB.SetValue(Canvas.ZIndexProperty, 0);
                    imageF.SetValue(Canvas.ZIndexProperty, 1);
                    visualF.StartAnimation(nameof(visualB.Offset), animations.CreateSlideAnimation(vZero, vector, Duration));
                }
                else
                {
                    imageB.SetValue(Canvas.ZIndexProperty, 1);
                    imageF.SetValue(Canvas.ZIndexProperty, 0);
                    visualB.StartAnimation(nameof(visualF.Offset), animations.CreateSlideAnimation(vZero, vector, Duration));
                }
            }
            else
            {
                if (isFrontVisible)
                {
                    imageB.SetValue(Canvas.ZIndexProperty, 1);
                    imageF.SetValue(Canvas.ZIndexProperty, 0);
                    visualB.StartAnimation(nameof(visualB.Offset), animations.CreateSlideAnimation(vector, vZero, Duration));
                }
                else
                {
                    imageB.SetValue(Canvas.ZIndexProperty, 0);
                    imageF.SetValue(Canvas.ZIndexProperty, 1);
                    visualF.StartAnimation(nameof(visualF.Offset), animations.CreateSlideAnimation(vector, vZero, Duration));
                }
            }
        }

        private void StackAndScaleAnimation(bool horizontally, bool fromLeftTop)
        {
            CompositionAnimationGroup animationGroupB = compositor.CreateAnimationGroup();
            CompositionAnimationGroup animationGroupF = compositor.CreateAnimationGroup();

            visualB.CenterPoint = new Vector3((visualB.Size.X / 2.0f), (visualB.Size.Y / 2.0f), 0.0f);
            visualF.CenterPoint = new Vector3((visualF.Size.X / 2.0f), (visualF.Size.Y / 2.0f), 0.0f);

            Vector3 small = new Vector3(0.7f, 0.7f, 7f);
            Vector3 big = new Vector3(1.4f, 1.4f, 1f);

            int dir = fromLeftTop ? -1 : 1;
            if (isFrontVisible)
            {
                imageB.Source = GetImage(ImageUri);
                visualB.Offset = new Vector3(0f, 0f, 0f);
            }
            else
            {
                imageF.Source = GetImage(ImageUri);
                visualF.Offset = new Vector3(0f, 0f, 0f);
            }
            Vector3 vector;
            if (horizontally)
            {
                vector = new Vector3(dir * ImageWidth, 0f, 0f);
            }
            else
            {
                vector = new Vector3(0f, dir * ImageHeight, 0f);
            }

            if (Direction == Direction.Next)
            {
                if (isFrontVisible)
                {
                    imageB.SetValue(Canvas.ZIndexProperty, 0);
                    imageF.SetValue(Canvas.ZIndexProperty, 1);

                    animationGroupB.Add(animations.CreateOpacityAnimation(0.5f, 1f, Duration));
                    animationGroupB.Add(animations.CreateScaleAnimation(small, vOne, Duration));

                    animationGroupF.Add(animations.CreateSlideAnimation(vZero, vector, Duration));

                    visualB.StartAnimationGroup(animationGroupB);
                    visualF.StartAnimationGroup(animationGroupF);
                }
                else
                {
                    imageB.SetValue(Canvas.ZIndexProperty, 1);
                    imageF.SetValue(Canvas.ZIndexProperty, 0);

                    animationGroupF.Add(animations.CreateOpacityAnimation(0.5f, 1f, Duration));
                    animationGroupF.Add(animations.CreateScaleAnimation(small, vOne, Duration));

                    animationGroupB.Add(animations.CreateSlideAnimation(vZero, vector, Duration));

                    visualB.StartAnimationGroup(animationGroupB);
                    visualF.StartAnimationGroup(animationGroupF);
                }
            }
            else
            {
                if (isFrontVisible)
                {
                    imageB.SetValue(Canvas.ZIndexProperty, 1);
                    imageF.SetValue(Canvas.ZIndexProperty, 0);

                    visualB.Opacity = 1;
                    visualB.Scale = vOne;
                    animationGroupB.Add(animations.CreateSlideAnimation(vector, vZero, Duration));

                    animationGroupF.Add(animations.CreateOpacityAnimation(1f, 0.5f, Duration));
                    animationGroupF.Add(animations.CreateScaleAnimation(vOne, small, Duration));

                    visualB.StartAnimationGroup(animationGroupB);
                    visualF.StartAnimationGroup(animationGroupF);
                }
                else
                {
                    imageB.SetValue(Canvas.ZIndexProperty, 0);
                    imageF.SetValue(Canvas.ZIndexProperty, 1);

                    visualF.Opacity = 1;
                    visualF.Scale = vOne;
                    animationGroupF.Add(animations.CreateSlideAnimation(vector, vZero, Duration));

                    animationGroupB.Add(animations.CreateOpacityAnimation(1f, 0.5f, Duration));
                    animationGroupB.Add(animations.CreateScaleAnimation(vOne, small, Duration));

                    visualB.StartAnimationGroup(animationGroupB);
                    visualF.StartAnimationGroup(animationGroupF);
                }
            }
        }

        private void ScaleAndOpacityAnimation()
        {
            CompositionAnimationGroup animationGroupB = compositor.CreateAnimationGroup();
            CompositionAnimationGroup animationGroupF = compositor.CreateAnimationGroup();

            visualB.CenterPoint = new Vector3((visualB.Size.X / 2.0f), (visualB.Size.Y / 2.0f), 0.0f);
            visualF.CenterPoint = new Vector3((visualF.Size.X / 2.0f), (visualF.Size.Y / 2.0f), 0.0f);

            Vector3 small = new Vector3(0.7f, 0.7f, 7f);
            Vector3 big = new Vector3(1.4f, 1.4f, 1f);

            if (Direction == Direction.Next)
            {
                if (isFrontVisible)
                {
                    imageB.Source = GetImage(ImageUri);

                    animationGroupB.Add(animations.CreateOpacityAnimation(0f, 1f, Duration));
                    animationGroupB.Add(animations.CreateScaleAnimation(small, vOne, Duration));
                    animationGroupF.Add(animations.CreateOpacityAnimation(1f, 0f, Duration));
                    animationGroupF.Add(animations.CreateScaleAnimation(vOne, big, Duration));

                    visualB.StartAnimationGroup(animationGroupB);
                    visualF.StartAnimationGroup(animationGroupF);
                }
                else
                {
                    imageF.Source = GetImage(ImageUri);

                    animationGroupF.Add(animations.CreateOpacityAnimation(0f, 1f, Duration));
                    animationGroupF.Add(animations.CreateScaleAnimation(small, vOne, Duration));
                    animationGroupB.Add(animations.CreateOpacityAnimation(1f, 0f, Duration));
                    animationGroupB.Add(animations.CreateScaleAnimation(vOne, big, Duration));

                    visualB.StartAnimationGroup(animationGroupB);
                    visualF.StartAnimationGroup(animationGroupF);
                }
            }
            else
            {
                if (isFrontVisible)
                {
                    imageB.Source = GetImage(ImageUri);

                    animationGroupB.Add(animations.CreateOpacityAnimation(0f, 1f, Duration));
                    animationGroupB.Add(animations.CreateScaleAnimation(big, vOne, Duration));
                    animationGroupF.Add(animations.CreateOpacityAnimation(1f, 0f, Duration));
                    animationGroupF.Add(animations.CreateScaleAnimation(vOne, small, Duration));

                    visualB.StartAnimationGroup(animationGroupB);
                    visualF.StartAnimationGroup(animationGroupF);
                }
                else
                {
                    imageF.Source = GetImage(ImageUri);

                    animationGroupF.Add(animations.CreateOpacityAnimation(0f, 1f, Duration));
                    animationGroupF.Add(animations.CreateScaleAnimation(big, vOne, Duration));
                    animationGroupB.Add(animations.CreateOpacityAnimation(1f, 0f, Duration));
                    animationGroupB.Add(animations.CreateScaleAnimation(vOne, small, Duration));

                    visualB.StartAnimationGroup(animationGroupB);
                    visualF.StartAnimationGroup(animationGroupF);
                }
            }
        }

        private BitmapImage GetImage(Uri uri)
        {
            var bmp = new BitmapImage();
            bmp.DecodePixelHeight = ImageWidth;
            bmp.DecodePixelWidth = ImageWidth;
            bmp.DecodePixelType = DecodePixelType.Logical;
            bmp.UriSource = uri;
            return bmp;
        }
    }
}
