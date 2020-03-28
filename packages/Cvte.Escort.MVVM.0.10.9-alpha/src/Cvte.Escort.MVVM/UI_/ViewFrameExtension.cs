using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using Cvte.Escort.Annotations;
#pragma warning disable 1591

namespace Cvte.Escort
{
    internal static class ViewFrameExtension
    {
        /// <summary>
        /// 检查 <paramref name="target"/> 是否有进入动画
        /// </summary>
        /// <param name="viewFrame"></param>
        /// <param name="target">UI界面</param>
        /// <returns>页面进入动画，无则返回空</returns>
        [CanBeNull]
        public static DoubleAnimation CheckInAnimation([NotNull] this ViewFrame viewFrame, [CanBeNull] UIElement target)
        {
            if (viewFrame == null)
            {
                throw new ArgumentNullException(nameof(viewFrame));
            }

            // 如果 target 无法转换为 IPageTransitionAnimation，则表示其没有实现这个接口，返回空
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (!(target is IPageTransitionAnimation animation))
            {
                return null;
            }

            if (animation.InOutMode == PageTransitionMode.OnlyOut)
            {
                return null;
            }

            return CreateAnimation(viewFrame, animation.InDirection);
        }

        /// <summary>
        /// 检查 <paramref name="target"/> 是否有退出动画
        /// </summary>
        /// <param name="viewFrame"></param>
        /// <param name="target">UI界面</param>
        /// <returns>页面退出动画，无则返回空</returns>
        [CanBeNull]
        public static DoubleAnimation CheckOutAnimation([NotNull] this ViewFrame viewFrame, [CanBeNull] UIElement target)
        {
            if (viewFrame == null)
            {
                throw new ArgumentNullException(nameof(viewFrame));
            }

            // 如果 target 无法转换为 IPageTransitionAnimation，则表示其没有实现这个接口，返回空
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (!(target is IPageTransitionAnimation animation))
            {
                return null;
            }

            if (animation.InOutMode == PageTransitionMode.OnlyIn)
            {
                return null;
            }

            return CreateAnimation(viewFrame, animation.OutDirection);
        }

        private static DoubleAnimation CreateAnimation(ViewFrame viewFrame, PageTransitionDirection direction)
        {
            var transitionTarget = viewFrame;

            // 初始化 Effect 
            var effect = new TransitionSlideInEffect
            {
                Texture2 = new ImageBrush(TakeSnap(transitionTarget)),
                Progress = 0
            };
            switch (direction)
            {
                case PageTransitionDirection.LeftToRight:
                    effect.SlideAmount = new Point(-1.0, 0.0);
                    break;
                case PageTransitionDirection.RightToLeft:
                    effect.SlideAmount = new Point(1.0, 0.0);
                    break;
                case PageTransitionDirection.TopToBottom:
                    effect.SlideAmount = new Point(0.0, -1.0);
                    break;
                case PageTransitionDirection.BottomToTop:
                    effect.SlideAmount = new Point(0.0, 1.0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            transitionTarget.Effect = effect;

            var doubleAnimation = new DoubleAnimation(0, 100, TimeSpan.FromMilliseconds(300))
            {
                EasingFunction = new PowerEase()
                {
                    EasingMode = EasingMode.EaseOut
                }
            };

            doubleAnimation.Completed += (sender, args) =>
            {
                transitionTarget.Effect = null;
            };

            return doubleAnimation;
        }

        /// <summary>
        /// 对 <paramref name="element"/> 进行截图
        /// </summary>
        /// <param name="element"></param>
        /// <returns>截图Image</returns>
        public static ImageSource TakeSnap([NotNull] FrameworkElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            // 云课件窗口大小 ： 844 580 

            double scale = 1;

            // 获取包括标题栏的 宽度 和 高度 
            Rect descendantBounds = VisualTreeHelper.GetDescendantBounds(element);
            var width = (int)descendantBounds.Width;
            var height = (int)descendantBounds.Height;

            if (!element.IsLoaded)
            {
                element.Arrange(new Rect(0, 0, width, height));
                element.Measure(new Size(width, height));
            }

            var scaleWidth = (int) (width * scale);
            var scaleHeight = (int) (height * scale);

            var bitmap = new RenderTargetBitmap(scaleWidth, scaleHeight, 96.0, 96.0, PixelFormats.Pbgra32);
            var rectangle = new Rectangle
            {
                Width = scaleWidth,
                Height = scaleHeight,

                Fill = new VisualBrush(element)
                {
                    Viewbox = new Rect(0, 0, width, height),
                    ViewboxUnits = BrushMappingMode.Absolute,
                }
            };

            rectangle.Measure(new Size(scaleWidth, scaleHeight));
            rectangle.Arrange(new Rect(new Size(scaleWidth, scaleHeight)));

            bitmap.Render(rectangle);
            return bitmap;
        }

    }
   
    /// <summary>
    /// 页面切换特效
    /// </summary>
    public class TransitionSlideInEffect : ShaderEffect
    {
        public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(TransitionSlideInEffect), 0);
        public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register("Progress", typeof(double), typeof(TransitionSlideInEffect), new UIPropertyMetadata(((double) (30D)), PixelShaderConstantCallback(0)));
        public static readonly DependencyProperty SlideAmountProperty = DependencyProperty.Register("SlideAmount", typeof(Point), typeof(TransitionSlideInEffect), new UIPropertyMetadata(new Point(1D, 0D), PixelShaderConstantCallback(1)));
        public static readonly DependencyProperty Texture2Property = ShaderEffect.RegisterPixelShaderSamplerProperty("Texture2", typeof(TransitionSlideInEffect), 1);

        /// <summary>
        /// 创建页面切换特效
        /// </summary>
        public TransitionSlideInEffect()
        {
            PixelShader pixelShader = new PixelShader();

            var uri = new Uri("pack://application:,,,/Cvte.Escort.MVVM;component/Resources/Transition_SlideInEffect.ps");    
            
            pixelShader.UriSource = uri;
            this.PixelShader = pixelShader;

            this.UpdateShaderValue(InputProperty);
            this.UpdateShaderValue(ProgressProperty);
            this.UpdateShaderValue(SlideAmountProperty);
            this.UpdateShaderValue(Texture2Property);
        }

        /// <summary>
        /// 获取或设置第一页的Brush
        /// </summary>
        public Brush Input
        {
            get => ((Brush) (this.GetValue(InputProperty)));
            set => this.SetValue(InputProperty, value);
        }

        /// <summary>
        /// 获取或设置切页进度 取值0~1
        /// </summary>
        public double Progress
        {
            get => ((double) (this.GetValue(ProgressProperty)));
            set => this.SetValue(ProgressProperty, value);
        }

        /// <summary>
        /// 获取或设置切页动画方向
        /// </summary>
        public Point SlideAmount
        {
            get => ((Point) (this.GetValue(SlideAmountProperty)));
            set => this.SetValue(SlideAmountProperty, value);
        }

        /// <summary>
        /// 获取或设置第二页的Brush
        /// </summary>
        public Brush Texture2
        {
            get => ((Brush) (this.GetValue(Texture2Property)));
            set => this.SetValue(Texture2Property, value);
        }
    }



}


