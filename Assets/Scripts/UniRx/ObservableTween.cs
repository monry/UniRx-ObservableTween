using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace UniRx
{
    // ReSharper disable once PartialTypeWithSinglePart
    [PublicAPI]
    public static partial class ObservableTween
    {
        public enum EaseType
        {
            Linear,
            InQuadratic,
            OutQuadratic,
            InOutQuadratic,
            InCubic,
            OutCubic,
            InOutCubic,
            InQuartic,
            OutQuartic,
            InOutQuartic,
            InQuintic,
            OutQuintic,
            InOutQuintic,
            InSinusoidal,
            OutSinusoidal,
            InOutSinusoidal,
            InExponential,
            OutExponential,
            InOutExponential,
            InCircular,
            OutCircular,
            InOutCircular,
            InBack,
            OutBack,
            InOutBack,
            InBounce,
            OutBounce,
            InOutBounce,
            InElastic,
            OutElastic,
            InOutElastic,
        }

        public enum LoopType
        {
            /// <summary>
            /// ループなし
            /// </summary>
            None,

            /// <summary>
            /// 同じ Easing を繰り返す
            /// </summary>
            Repeat,

            /// <summary>
            /// 同じ Easing を start/finish を入れ替えて繰り返す
            /// </summary>
            PingPong,

            /// <summary>
            /// 行きの Easing に対応する帰りの Easing を繰り返す
            /// </summary>
            Mirror,
        }

        private static readonly Dictionary<EaseType, EaseType> MirrorEaseTypeMap = new Dictionary<EaseType, EaseType>()
        {
            {EaseType.Linear, EaseType.Linear},
            {EaseType.InQuadratic, EaseType.OutQuadratic},
            {EaseType.OutQuadratic, EaseType.InQuadratic},
            {EaseType.InOutQuadratic, EaseType.InOutQuadratic},
            {EaseType.InCubic, EaseType.OutCubic},
            {EaseType.OutCubic, EaseType.InCubic},
            {EaseType.InOutCubic, EaseType.InOutCubic},
            {EaseType.InQuartic, EaseType.OutQuartic},
            {EaseType.OutQuartic, EaseType.InQuartic},
            {EaseType.InOutQuartic, EaseType.InOutQuartic},
            {EaseType.InQuintic, EaseType.OutQuintic},
            {EaseType.OutQuintic, EaseType.InQuintic},
            {EaseType.InOutQuintic, EaseType.InOutQuintic},
            {EaseType.InSinusoidal, EaseType.OutSinusoidal},
            {EaseType.OutSinusoidal, EaseType.InSinusoidal},
            {EaseType.InOutSinusoidal, EaseType.InOutSinusoidal},
            {EaseType.InExponential, EaseType.OutExponential},
            {EaseType.OutExponential, EaseType.InExponential},
            {EaseType.InOutExponential, EaseType.InOutExponential},
            {EaseType.InCircular, EaseType.OutCircular},
            {EaseType.OutCircular, EaseType.InCircular},
            {EaseType.InOutCircular, EaseType.InOutCircular},
            {EaseType.InBack, EaseType.OutBack},
            {EaseType.OutBack, EaseType.InBack},
            {EaseType.InOutBack, EaseType.InOutBack},
            {EaseType.InBounce, EaseType.OutBounce},
            {EaseType.OutBounce, EaseType.InBounce},
            {EaseType.InOutBounce, EaseType.InOutBounce},
            {EaseType.InElastic, EaseType.OutElastic},
            {EaseType.OutElastic, EaseType.InElastic},
            {EaseType.InOutElastic, EaseType.InOutElastic},
        };

        private static readonly Dictionary<Type, Type> OperationalStructMap = new Dictionary<Type, Type>()
        {
            {typeof(int), typeof(OperationalInt)},
            {typeof(float), typeof(OperationalFloat)},
            {typeof(Vector2), typeof(OperationalVector2)},
            {typeof(Vector3), typeof(OperationalVector3)},
        };

        public static IObservable<T> Tween<T>(T start, T finish, float duration, EaseType easeType, LoopType loopType = LoopType.None, Action onCompleteTween = null, bool ignoreTimeScale = false) where T : struct
        {
            return Tween(() => start, () => finish, () => duration, easeType, loopType, onCompleteTween, ignoreTimeScale);
        }

        public static IObservable<T> Tween<T>(T start, T finish, Func<float> duration, EaseType easeType, LoopType loopType = LoopType.None, Action onCompleteTween = null, bool ignoreTimeScale = false) where T : struct
        {
            return Tween(() => start, () => finish, duration, easeType, loopType, onCompleteTween, ignoreTimeScale);
        }

        public static IObservable<T> Tween<T>(Func<T> start, Func<T> finish, float duration, EaseType easeType, LoopType loopType = LoopType.None, Action onCompleteTween = null, bool ignoreTimeScale = false) where T : struct
        {
            return Tween(start, finish, () => duration, easeType, loopType, onCompleteTween, ignoreTimeScale);
        }

        public static IObservable<T> Tween<T>(Func<T> start, Func<T> finish, Func<float> duration, EaseType easeType, LoopType loopType = LoopType.None, Action onCompleteTween = null, bool ignoreTimeScale = false) where T : struct
        {
            return Tween(
                () => Activator.CreateInstance(OperationalStructMap[typeof(T)], start()) as OperationalStructBase<T>,
                () => Activator.CreateInstance(OperationalStructMap[typeof(T)], finish()) as OperationalStructBase<T>,
                duration,
                easeType,
                loopType,
                onCompleteTween,
                ignoreTimeScale
            );
        }

        private struct TweenInformation<T> where T : struct
        {
            public float Time { get; set; }

            public float StartTime { get; }

            public OperationalStructBase<T> Start { get; }

            public OperationalStructBase<T> Finish { get; }

            public float Duration { get; }

            public EaseType EaseType { get; }

            public TweenInformation(float startTime, OperationalStructBase<T> start, OperationalStructBase<T> finish, float duration, EaseType easeType, out T startValue, out T finishValue)
            {
                Time = startTime;
                StartTime = startTime;
                Start = start;
                Finish = finish;
                Duration = duration;
                EaseType = easeType;
                startValue = start.Value;
                finishValue = finish.Value;
            }
        }

        private static IObservable<T> Tween<T>(Func<OperationalStructBase<T>> start, Func<OperationalStructBase<T>> finish, Func<float> duration, EaseType easeType, LoopType loopType, Action onCompleteTween, bool ignoreTimeScale) where T : struct
        {
            T startValue = default;
            T finishValue = default;
            onCompleteTween = onCompleteTween ?? (() => { });

            IDisposable ReturnStartValue(IObserver<T> observer)
            {
                observer.OnNext(startValue);
                return null;
            }

            IDisposable ReturnFinishValue(IObserver<T> observer)
            {
                observer.OnNext(finishValue);
                return null;
            }

            IObservable<T> stream = Observable.Empty<TweenInformation<T>>()
                // Repeat() のために、毎回初期値を生成
                .StartWith(() => new TweenInformation<T>(ignoreTimeScale ? Time.unscaledTime : Time.time, start(), finish(), duration(), easeType, out startValue, out finishValue))
                // Update のストリームに変換
                .SelectMany(information => Observable.Interval(TimeSpan.FromMilliseconds(1), ignoreTimeScale ? Scheduler.MainThreadIgnoreTimeScale : Scheduler.MainThread).Do(_ => information.Time = ignoreTimeScale ? Time.unscaledTime : Time.time - information.StartTime).Select(_ => information))
                // Tween 時間が処理時間よりも小さい間流し続ける
                .TakeWhile(information => information.Time <= information.Duration)
                // 実際の Easing 処理実行
                .Select(information => Easing(information.Time, information.Start, (information.Finish - information.Start), information.Duration, information.EaseType).Value)
                // 最終フレームの値を確実に流すために OnCompleted が来たら値を一つ流すストリームに繋ぐ
                // 1回分の Tween が終わったらコールバックを呼ぶ
                .Concat(Observable.Create((Func<IObserver<T>, IDisposable>) ReturnFinishValue).Take(1).Do(_ => onCompleteTween()));
            switch (loopType)
            {
                case LoopType.None:
                    // Do nothing.
                    break;
                case LoopType.Repeat:
                    stream = stream.Repeat();
                    break;
                case LoopType.PingPong:
                    stream = stream
                        .Concat(
                            Observable.Empty<TweenInformation<T>>()
                                // Repeat() のために、毎回初期値を生成
                                .StartWith(() => new TweenInformation<T>(ignoreTimeScale ? Time.unscaledTime : Time.time, start(), finish(), duration(), easeType, out startValue, out finishValue))
                                // Update のストリームに変換
                                .SelectMany(information => Observable.Interval(TimeSpan.FromMilliseconds(1), ignoreTimeScale ? Scheduler.MainThreadIgnoreTimeScale : Scheduler.MainThread).Do(_ => information.Time = ignoreTimeScale ? Time.unscaledTime : Time.time - information.StartTime).Select(_ => information))
                                // Tween 時間が処理時間よりも小さい間流し続ける
                                .TakeWhile(information => information.Time <= information.Duration)
                                // start と finish を入れ替えて、実際の Easing 処理実行
                                .Select(information => Easing(information.Time, information.Finish, (information.Start - information.Finish), information.Duration, information.EaseType).Value)
                                // 最終フレームの値を確実に流すために OnCompleted が来たら最終値を一つ流すストリームに繋ぐ
                                // 1回分の Tween が終わったらコールバックを呼ぶ
                                .Concat(Observable.Create((Func<IObserver<T>, IDisposable>) ReturnStartValue).Take(1).Do(_ => onCompleteTween()))
                        )
                        .Repeat();
                    break;
                case LoopType.Mirror:
                    stream = stream
                        .Concat(
                            Observable.Empty<TweenInformation<T>>()
                                // Repeat() のために、毎回初期値を生成
                                .StartWith(() => new TweenInformation<T>(ignoreTimeScale ? Time.unscaledTime : Time.time, start(), finish(), duration(), easeType, out startValue, out finishValue))
                                // Update のストリームに変換
                                .SelectMany(information => Observable.Interval(TimeSpan.FromMilliseconds(1), ignoreTimeScale ? Scheduler.MainThreadIgnoreTimeScale : Scheduler.MainThread).Do(_ => information.Time = ignoreTimeScale ? Time.unscaledTime : Time.time - information.StartTime).Select(_ => information))
                                // Tween 時間が処理時間よりも小さい間流し続ける
                                .TakeWhile(information => information.Time <= information.Duration)
                                // start と finish を入れ替えて、実際の Easing 処理実行
                                .Select(information => Easing(information.Time, information.Finish, (information.Start - information.Finish), information.Duration, MirrorEaseTypeMap[information.EaseType]).Value)
                                // 最終フレームの値を確実に流すために OnCompleted が来たら最終値を一つ流すストリームに繋ぐ
                                // 1回分の Tween が終わったらコールバックを呼ぶ
                                .Concat(Observable.Create((Func<IObserver<T>, IDisposable>) ReturnStartValue).Take(1).Do(_ => onCompleteTween()))
                        )
                        .Repeat();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(loopType), loopType, null);
            }

            return stream;
        }

        private static OperationalStructBase<T> Easing<T>(float time, OperationalStructBase<T> initial, OperationalStructBase<T> delta, float duration, EaseType easeType) where T : struct
        {
            if (!EasingFunctions<T>.EasingFunctionMap.ContainsKey(easeType))
            {
                throw new ArgumentException($"EaseType: '{easeType.ToString()}' does not implement yet.");
            }

            if (time <= 0.0f)
            {
                return initial;
            }

            if (time >= duration)
            {
                return initial + delta;
            }

            return EasingFunctions<T>.EasingFunctionMap[easeType](time, initial, delta, duration);
        }

        private static class EasingFunctions<T> where T : struct
        {
            private const float EaseBackThreshold = 1.70158f;

            public static readonly Dictionary<EaseType, Func<float, OperationalStructBase<T>, OperationalStructBase<T>, float, OperationalStructBase<T>>> EasingFunctionMap = new Dictionary<EaseType, Func<float, OperationalStructBase<T>, OperationalStructBase<T>, float, OperationalStructBase<T>>>()
            {
                {EaseType.Linear, EaseLinear},
                {EaseType.InQuadratic, EaseInQuadratic},
                {EaseType.OutQuadratic, EaseOutQuadratic},
                {EaseType.InOutQuadratic, EaseInOutQuadratic},
                {EaseType.InCubic, EaseInCubic},
                {EaseType.OutCubic, EaseOutCubic},
                {EaseType.InOutCubic, EaseInOutCubic},
                {EaseType.InQuartic, EaseInQuartic},
                {EaseType.OutQuartic, EaseOutQuartic},
                {EaseType.InOutQuartic, EaseInOutQuartic},
                {EaseType.InQuintic, EaseInQuintic},
                {EaseType.OutQuintic, EaseOutQuintic},
                {EaseType.InOutQuintic, EaseInOutQuintic},
                {EaseType.InSinusoidal, EaseInSinusoidal},
                {EaseType.OutSinusoidal, EaseOutSinusoidal},
                {EaseType.InOutSinusoidal, EaseInOutSinusoidal},
                {EaseType.InExponential, EaseInExponential},
                {EaseType.OutExponential, EaseOutExponential},
                {EaseType.InOutExponential, EaseInOutExponential},
                {EaseType.InCircular, EaseInCircular},
                {EaseType.OutCircular, EaseOutCircular},
                {EaseType.InOutCircular, EaseInOutCircular},
                {EaseType.InBack, EaseInBack},
                {EaseType.OutBack, EaseOutBack},
                {EaseType.InOutBack, EaseInOutBack},
                {EaseType.InBounce, EaseInBounce},
                {EaseType.OutBounce, EaseOutBounce},
                {EaseType.InOutBounce, EaseInOutBounce},
                {EaseType.InElastic, EaseInElastic},
                {EaseType.OutElastic, EaseOutElastic},
                {EaseType.InOutElastic, EaseInOutElastic},
            };

            private static OperationalStructBase<T> EaseLinear(float time, OperationalStructBase<T> initial, OperationalStructBase<T> delta, float duration)
            {
                return delta * time / duration + initial;
            }

            private static OperationalStructBase<T> EaseInQuadratic(float time, OperationalStructBase<T> initial, OperationalStructBase<T> delta, float duration)
            {
                time /= duration;
                return delta * time * time + initial;
            }

            private static OperationalStructBase<T> EaseOutQuadratic(float time, OperationalStructBase<T> initial, OperationalStructBase<T> delta, float duration)
            {
                time /= duration;
                return -delta * time * (time - 2.0f) + initial;
            }

            private static OperationalStructBase<T> EaseInOutQuadratic(float time, OperationalStructBase<T> initial, OperationalStructBase<T> delta, float duration)
            {
                time /= duration / 2.0f;
                if (time <= 1.0f)
                {
                    return delta / 2.0f * time * time + initial;
                }

                time -= 1.0f;
                return -delta / 2.0f * (time * (time - 2.0f) - 1.0f) + initial;
            }

            private static OperationalStructBase<T> EaseInCubic(float time, OperationalStructBase<T> initial, OperationalStructBase<T> delta, float duration)
            {
                time /= duration;
                return delta * Mathf.Pow(time, 3.0f) + initial;
            }

            private static OperationalStructBase<T> EaseOutCubic(float time, OperationalStructBase<T> initial, OperationalStructBase<T> delta, float duration)
            {
                time /= duration;
                time = time - 1.0f;
                return delta * (Mathf.Pow(time, 3.0f) + 1.0f) + initial;
            }

            private static OperationalStructBase<T> EaseInOutCubic(float time, OperationalStructBase<T> initial, OperationalStructBase<T> delta, float duration)
            {
                time /= duration / 2.0f;
                if (time <= 1.0f)
                {
                    return delta / 2.0f * Mathf.Pow(time, 3.0f) + initial;
                }

                time -= 2.0f;
                return delta / 2.0f * (Mathf.Pow(time, 3.0f) + 2.0f) + initial;
            }

            private static OperationalStructBase<T> EaseInQuartic(float time, OperationalStructBase<T> initial, OperationalStructBase<T> delta, float duration)
            {
                time /= duration;
                return delta * Mathf.Pow(time, 4.0f) + initial;
            }

            private static OperationalStructBase<T> EaseOutQuartic(float time, OperationalStructBase<T> initial, OperationalStructBase<T> delta, float duration)
            {
                time /= duration;
                time -= 1.0f;
                return -delta * (Mathf.Pow(time, 4.0f) - 1.0f) + initial;
            }

            private static OperationalStructBase<T> EaseInOutQuartic(float time, OperationalStructBase<T> initial, OperationalStructBase<T> delta, float duration)
            {
                time /= duration / 2.0f;
                if (time <= 1.0f)
                {
                    return delta / 2.0f * Mathf.Pow(time, 4.0f) + initial;
                }

                time -= 2.0f;
                return -delta * 2.0f * (Mathf.Pow(time, 4.0f) - 2.0f) + initial;
            }

            private static OperationalStructBase<T> EaseInQuintic(float time, OperationalStructBase<T> initial, OperationalStructBase<T> delta, float duration)
            {
                time /= duration;
                return delta * Mathf.Pow(time, 5.0f) + initial;
            }

            private static OperationalStructBase<T> EaseOutQuintic(float time, OperationalStructBase<T> initial, OperationalStructBase<T> delta, float duration)
            {
                time /= duration;
                time -= 1.0f;
                return delta * (Mathf.Pow(time, 5.0f) + 1.0f) + initial;
            }

            private static OperationalStructBase<T> EaseInOutQuintic(float time, OperationalStructBase<T> initial, OperationalStructBase<T> delta, float duration)
            {
                time /= duration / 2.0f;
                if (time <= 1.0f)
                {
                    return delta / 2.0f * Mathf.Pow(time, 5.0f) + initial;
                }

                time -= 2.0f;
                return delta / 2.0f * (Mathf.Pow(time, 5.0f) + 2.0f) + initial;
            }

            private static OperationalStructBase<T> EaseInSinusoidal(float time, OperationalStructBase<T> initial, OperationalStructBase<T> delta, float duration)
            {
                return -delta * Mathf.Cos(time / duration * (Mathf.PI / 2.0f)) + delta + initial;
            }

            private static OperationalStructBase<T> EaseOutSinusoidal(float time, OperationalStructBase<T> initial, OperationalStructBase<T> delta, float duration)
            {
                return delta * Mathf.Sin(time / duration * (Mathf.PI / 2.0f)) + initial;
            }

            private static OperationalStructBase<T> EaseInOutSinusoidal(float time, OperationalStructBase<T> initial, OperationalStructBase<T> delta, float duration)
            {
                return -delta / 2.0f * (Mathf.Cos(Mathf.PI * time / duration) + 1.0f) + initial;
            }

            private static OperationalStructBase<T> EaseInExponential(float time, OperationalStructBase<T> initial, OperationalStructBase<T> delta, float duration)
            {
                return delta * Mathf.Pow(2.0f, 10.0f * (time / duration - 1.0f)) + initial;
            }

            private static OperationalStructBase<T> EaseOutExponential(float time, OperationalStructBase<T> initial, OperationalStructBase<T> delta, float duration)
            {
                return delta * (-Mathf.Pow(2.0f, -10.0f * time / duration) + 1.0f) + initial;
            }

            private static OperationalStructBase<T> EaseInOutExponential(float time, OperationalStructBase<T> initial, OperationalStructBase<T> delta, float duration)
            {
                time /= duration / 2.0f;
                if (time <= 1.0f)
                {
                    return delta / 2.0f * Mathf.Pow(2.0f, 10.0f * (time - 1.0f)) + initial;
                }

                time -= 1.0f;
                return delta / 2.0f * (-Mathf.Pow(2.0f, -10.0f * time) + 2.0f) + initial;
            }

            private static OperationalStructBase<T> EaseInCircular(float time, OperationalStructBase<T> initial, OperationalStructBase<T> delta, float duration)
            {
                time /= duration;
                return -delta * (Mathf.Sqrt(1 - Mathf.Pow(time, 2.0f)) + 1.0f) + initial;
            }

            private static OperationalStructBase<T> EaseOutCircular(float time, OperationalStructBase<T> initial, OperationalStructBase<T> delta, float duration)
            {
                time /= duration;
                time -= 1.0f;
                return delta * Mathf.Sqrt(1 - Mathf.Pow(time, 2.0f)) + initial;
            }

            private static OperationalStructBase<T> EaseInOutCircular(float time, OperationalStructBase<T> initial, OperationalStructBase<T> delta, float duration)
            {
                time /= duration / 2.0f;
                if (time <= 1.0f)
                {
                    return -delta / 2.0f * (Mathf.Sqrt(1 - Mathf.Pow(time, 2.0f)) - 1.0f) + initial;
                }

                time -= 2.0f;
                return delta / 2.0f * (Mathf.Sqrt(1 - Mathf.Pow(time, 2.0f)) + 1.0f) + initial;
            }

            private static OperationalStructBase<T> EaseInBack(float time, OperationalStructBase<T> initial, OperationalStructBase<T> delta, float duration)
            {
                time /= duration;
                return delta * Mathf.Pow(time, 2.0f) * ((EaseBackThreshold + 1.0f) * time - EaseBackThreshold) + initial;
            }

            private static OperationalStructBase<T> EaseOutBack(float time, OperationalStructBase<T> initial, OperationalStructBase<T> delta, float duration)
            {
                time /= duration;
                time -= 1.0f;
                return delta * (Mathf.Pow(time, 2.0f) * ((EaseBackThreshold + 1.0f) * time + EaseBackThreshold) + 1.0f) + initial;
            }

            private static OperationalStructBase<T> EaseInOutBack(float time, OperationalStructBase<T> initial, OperationalStructBase<T> delta, float duration)
            {
                const float threshold = EaseBackThreshold * 1.525f;
                time /= duration / 2.0f;
                if (time <= 1.0f)
                {
                    return delta / 2.0f * (Mathf.Pow(time, 2.0f) * ((threshold + 1.0f) * time - threshold)) + initial;
                }

                time -= 2.0f;
                return delta / 2.0f * (Mathf.Pow(time, 2.0f) * ((threshold + 1.0f) * time + threshold) + 2.0f) + initial;
            }

            private static OperationalStructBase<T> EaseInBounce(float time, OperationalStructBase<T> initial, OperationalStructBase<T> delta, float duration)
            {
                return delta - EaseOutBounce(duration - time, default, delta, duration) + initial;
            }

            private static OperationalStructBase<T> EaseOutBounce(float time, OperationalStructBase<T> initial, OperationalStructBase<T> delta, float duration)
            {
                time /= duration;
                if (time <= (1.0f / 2.75f))
                {
                    return delta * (7.5625f * Mathf.Pow(time, 2.0f)) + initial;
                }

                if (time <= (2.0f / 2.75f))
                {
                    time -= (1.5f / 2.75f);
                    return delta * (7.5625f * Mathf.Pow(time, 2.0f) + 0.75f) + initial;
                }

                if (time <= (2.5f / 2.75f))
                {
                    time -= (2.25f / 2.75f);
                    return delta * (7.5625f * Mathf.Pow(time, 2.0f) + 0.9375f) + initial;
                }

                time -= (2.625f / 2.75f);
                return delta * (7.5625f * Mathf.Pow(time, 2.0f) + 0.984375f) + initial;
            }

            private static OperationalStructBase<T> EaseInOutBounce(float time, OperationalStructBase<T> initial, OperationalStructBase<T> delta, float duration)
            {
                if (time <= duration / 2.0f)
                {
                    return EaseInBounce(time * 2.0f, default, delta, duration) * 0.5f + initial;
                }

                return EaseOutBounce(time * 2.0f - duration, default, delta, duration) * 0.5f + delta * 0.5f + initial;
            }

            private static OperationalStructBase<T> EaseInElastic(float time, OperationalStructBase<T> initial, OperationalStructBase<T> delta, float duration)
            {
                time /= duration;
                if (Mathf.Approximately(time, 1.0f))
                {
                    return initial + delta;
                }

                time -= 1.0f;
                var p = duration * 0.3f;
                var s = p / 4.0f;
                return -(delta * Mathf.Pow(2.0f, 10.0f * time) * Mathf.Sin((time * duration - s) * (2.0f * Mathf.PI) / p)) + initial;
            }

            private static OperationalStructBase<T> EaseOutElastic(float time, OperationalStructBase<T> initial, OperationalStructBase<T> delta, float duration)
            {
                time /= duration;
                if (Mathf.Approximately(time, 1.0f))
                {
                    return initial + delta;
                }

                var p = duration * 0.3f;
                var s = p / 4.0f;
                return delta * Mathf.Pow(2.0f, -10.0f * time) * Mathf.Sin((time * duration - s) * (2.0f * Mathf.PI) / p) + delta + initial;
            }

            private static OperationalStructBase<T> EaseInOutElastic(float time, OperationalStructBase<T> initial, OperationalStructBase<T> delta, float duration)
            {
                time /= duration / 2.0f;
                if (Mathf.Approximately(time, 2.0f))
                {
                    return initial + delta;
                }

                time -= 1.0f;
                var p = duration * (0.3f * 1.5f);
                var s = p / 4.0f;
                if (time <= 0.0f)
                {
                    return delta * Mathf.Pow(2.0f, 10.0f * time) * Mathf.Sin((time * duration - s) * (2.0f * Mathf.PI) / p) * -0.5f + initial;
                }

                return delta * Mathf.Pow(2.0f, -10.0f * time) * Mathf.Sin((time * duration - s) * (2.0f * Mathf.PI) / p) * 0.5f + delta + initial;
            }
        }

        // XXX: 本当は struct にした方がコストが低いが、 abstrcat クラスで operator を定義することで記述を柔軟にしたかったので class 定義にしている
        private abstract class OperationalStructBase<T> where T : struct
        {
            public abstract T Value { get; set; }

            protected abstract OperationalStructBase<T> Add(OperationalStructBase<T> value);

            protected abstract OperationalStructBase<T> Subtract(OperationalStructBase<T> value);

            protected abstract OperationalStructBase<T> Multiply(float value);

            protected abstract OperationalStructBase<T> Divide(float value);

            protected abstract int Compare(OperationalStructBase<T> value);

            public static OperationalStructBase<T> operator +(OperationalStructBase<T> a, OperationalStructBase<T> b)
            {
                return a.Add(b);
            }

            public static OperationalStructBase<T> operator -(OperationalStructBase<T> a, OperationalStructBase<T> b)
            {
                return a.Subtract(b);
            }

            public static OperationalStructBase<T> operator -(OperationalStructBase<T> a)
            {
                return a.Multiply(-1.0f);
            }

            public static OperationalStructBase<T> operator *(OperationalStructBase<T> a, float b)
            {
                return a.Multiply(b);
            }

            public static OperationalStructBase<T> operator /(OperationalStructBase<T> a, float b)
            {
                return a.Divide(b);
            }

            public static bool operator <(OperationalStructBase<T> a, OperationalStructBase<T> b)
            {
                return a.Compare(b) > 0;
            }

            public static bool operator >(OperationalStructBase<T> a, OperationalStructBase<T> b)
            {
                return a.Compare(b) < 0;
            }
        }

        private class OperationalInt : OperationalStructBase<int>
        {
            public sealed override int Value { get; set; }

            protected override OperationalStructBase<int> Add(OperationalStructBase<int> value)
            {
                return new OperationalInt(Value + value.Value);
            }

            protected override OperationalStructBase<int> Subtract(OperationalStructBase<int> value)
            {
                return new OperationalInt(Value - value.Value);
            }

            protected override OperationalStructBase<int> Multiply(float value)
            {
                return new OperationalInt((int) (Value * value));
            }

            protected override OperationalStructBase<int> Divide(float value)
            {
                return new OperationalInt((int) (Value / value));
            }

            protected override int Compare(OperationalStructBase<int> value)
            {
                return Value > value.Value ? 1 : -1;
            }

            public OperationalInt(int value)
            {
                Value = value;
            }
        }

        private class OperationalFloat : OperationalStructBase<float>
        {
            public sealed override float Value { get; set; }

            protected override OperationalStructBase<float> Add(OperationalStructBase<float> value)
            {
                return new OperationalFloat(Value + value.Value);
            }

            protected override OperationalStructBase<float> Subtract(OperationalStructBase<float> value)
            {
                return new OperationalFloat(Value - value.Value);
            }

            protected override OperationalStructBase<float> Multiply(float value)
            {
                return new OperationalFloat(Value * value);
            }

            protected override OperationalStructBase<float> Divide(float value)
            {
                return new OperationalFloat(Value / value);
            }

            protected override int Compare(OperationalStructBase<float> value)
            {
                return Value > value.Value ? 1 : -1;
            }

            public OperationalFloat(float value)
            {
                Value = value;
            }
        }

        private class OperationalVector2 : OperationalStructBase<Vector2>
        {
            public sealed override Vector2 Value { get; set; }

            protected override OperationalStructBase<Vector2> Add(OperationalStructBase<Vector2> value)
            {
                return new OperationalVector2(Value + value.Value);
            }

            protected override OperationalStructBase<Vector2> Subtract(OperationalStructBase<Vector2> value)
            {
                return new OperationalVector2(Value - value.Value);
            }

            protected override OperationalStructBase<Vector2> Multiply(float value)
            {
                return new OperationalVector2(Value * value);
            }

            protected override OperationalStructBase<Vector2> Divide(float value)
            {
                return new OperationalVector2(Value / value);
            }

            protected override int Compare(OperationalStructBase<Vector2> value)
            {
                return Value.magnitude > value.Value.magnitude ? 1 : -1;
            }

            public OperationalVector2(Vector2 value)
            {
                Value = value;
            }
        }

        private class OperationalVector3 : OperationalStructBase<Vector3>
        {
            public sealed override Vector3 Value { get; set; }

            protected override OperationalStructBase<Vector3> Add(OperationalStructBase<Vector3> value)
            {
                return new OperationalVector3(Value + value.Value);
            }

            protected override OperationalStructBase<Vector3> Subtract(OperationalStructBase<Vector3> value)
            {
                return new OperationalVector3(Value - value.Value);
            }

            protected override OperationalStructBase<Vector3> Multiply(float value)
            {
                return new OperationalVector3(Value * value);
            }

            protected override OperationalStructBase<Vector3> Divide(float value)
            {
                return new OperationalVector3(Value / value);
            }

            protected override int Compare(OperationalStructBase<Vector3> value)
            {
                return Value.magnitude > value.Value.magnitude ? 1 : -1;
            }

            public OperationalVector3(Vector3 value)
            {
                Value = value;
            }
        }
    }
}
