﻿using System.Windows;
using System.Windows.Media.Animation;

namespace Celestial.UIToolkit.Media.Animations
{

    /// <summary>
    /// Defines an abstract base class for a From/To/By animation,
    /// which, in comparison to the <see cref="FromToByAnimationBase{T}"/>,
    /// additionally uses an easing function.
    /// </summary>
    /// <typeparam name="T">The type which is being animated by the animation.</typeparam>
    public abstract class EasingFromToByAnimationBase<T> : FromToByAnimationBase<T>
    {
        
        /// <summary>
        /// Identifies the <see cref="EasingFunction"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty EasingFunctionProperty = DependencyProperty.Register(
            nameof(EasingFunction), typeof(IEasingFunction), typeof(EasingFromToByAnimationBase<T>));

        /// <summary>
        /// Gets or sets the easing function applied to this animation.
        /// </summary>
        public IEasingFunction EasingFunction
        {
            get { return (IEasingFunction)GetValue(EasingFunctionProperty); }
            set { SetValue(EasingFunctionProperty, value); }
        }

        /// <summary>
        /// Applies the <see cref="EasingFunction"/> to the <paramref name="progress"/>
        /// and then calls <see cref="InterpolateValueCore(T, T, double)"/>,
        /// to get the desired interpolation result.
        /// </summary>
        /// <param name="from">The value from which the animation starts.</param>
        /// <param name="to">The value at which the animation ends.</param>
        /// <param name="progress">
        /// A value between 0.0 and 1.0, inclusive, that specifies the percentage of time
        /// that has elapsed during this animation.
        /// </param>
        /// <returns>The output value of the interpolation, given the specified values.</returns>
        protected override sealed T InterpolateValue(T from, T to, double progress)
        {
            if (this.EasingFunction != null)
            {
                progress = this.EasingFunction.Ease(progress);
            }
            return this.InterpolateValueCore(from, to, progress);
        }

        /// <summary>
        /// Returns the result of the interpolation of the two <paramref name="from"/> and <paramref name="to"/>
        /// values at the provided progress increment.
        /// </summary>
        /// <param name="from">The value from which the animation starts.</param>
        /// <param name="to">The value at which the animation ends.</param>
        /// <param name="progress">
        /// A value between 0.0 and 1.0, inclusive, that specifies the percentage of time
        /// that has elapsed during this animation.
        /// The local <see cref="EasingFunction"/> is already applied to this value.
        /// </param>
        /// <returns>The output value of the interpolation, given the specified values.</returns>
        protected abstract T InterpolateValueCore(T from, T to, double progress);

    }

}