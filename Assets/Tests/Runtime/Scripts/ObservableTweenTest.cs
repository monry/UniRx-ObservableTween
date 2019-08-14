using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UniRx
{
    public class ObservableTweenTest
    {
        [UnityTest]
        public IEnumerator IntTest()
        {
            var value = 0;
            ObservableTween.Tween(0, 100, 1.0f, ObservableTween.EaseType.Linear).Subscribe(x => value = x);
            yield return new WaitForSeconds(0.5f);
            Assert.GreaterOrEqual(value, 40);
            Assert.LessOrEqual(value, 60);
        }

        [UnityTest]
        public IEnumerator FloatTest()
        {
            var value = 0.0f;
            ObservableTween.Tween(0.0f, 100.0f, 1.0f, ObservableTween.EaseType.Linear).Subscribe(x => value = x);
            yield return new WaitForSeconds(0.5f);
            Assert.GreaterOrEqual(value, 40.0f);
            Assert.LessOrEqual(value, 60.0f);
        }

        [UnityTest]
        public IEnumerator Vector2Test()
        {
            var value = Vector2.zero;
            ObservableTween.Tween(Vector2.zero, Vector2.one, 1.0f, ObservableTween.EaseType.Linear).Subscribe(x => value = x);
            yield return new WaitForSeconds(0.5f);
            Assert.GreaterOrEqual(value.x, 0.4f);
            Assert.GreaterOrEqual(value.y, 0.4f);
            Assert.LessOrEqual(value.x, 0.6f);
            Assert.LessOrEqual(value.y, 0.6f);
        }

        [UnityTest]
        public IEnumerator Vector3Test()
        {
            var value = Vector3.zero;
            ObservableTween.Tween(Vector3.zero, Vector3.one, 1.0f, ObservableTween.EaseType.Linear).Subscribe(x => value = x);
            yield return new WaitForSeconds(0.5f);
            Assert.GreaterOrEqual(value.x, 0.4f);
            Assert.GreaterOrEqual(value.y, 0.4f);
            Assert.GreaterOrEqual(value.z, 0.4f);
            Assert.LessOrEqual(value.x, 0.6f);
            Assert.LessOrEqual(value.y, 0.6f);
            Assert.LessOrEqual(value.z, 0.6f);
        }

        [UnityTest]
        public IEnumerator EaseBackTest()
        {
            {
                var minValue = 0;
                var maxValue = 0;
                ObservableTween
                    .Tween(0, 100, 1.0f, ObservableTween.EaseType.InBack)
                    .Subscribe(
                        x =>
                        {
                            if (x < minValue)
                            {
                                minValue = x;
                            }
                            if (x > maxValue)
                            {
                                maxValue = x;
                            }
                        }
                    );
                yield return new WaitForSeconds(1.5f);
                Assert.Less(minValue, 0);
                Assert.AreEqual(maxValue, 100);
            }

            {
                var minValue = 0;
                var maxValue = 0;
                ObservableTween
                    .Tween(0, 100, 1.0f, ObservableTween.EaseType.OutBack)
                    .Subscribe(
                        x =>
                        {
                            if (x < minValue)
                            {
                                minValue = x;
                            }
                            if (x > maxValue)
                            {
                                maxValue = x;
                            }
                        }
                    );
                yield return new WaitForSeconds(1.5f);
                Assert.AreEqual(minValue, 0);
                Assert.Greater(maxValue, 100);
            }

            {
                var minValue = 0;
                var maxValue = 0;
                ObservableTween
                    .Tween(0, 100, 1.0f, ObservableTween.EaseType.InOutBack)
                    .Subscribe(
                        x =>
                        {
                            if (x < minValue)
                            {
                                minValue = x;
                            }
                            if (x > maxValue)
                            {
                                maxValue = x;
                            }
                        }
                    );
                yield return new WaitForSeconds(1.5f);
                Assert.Less(minValue, 0);
                Assert.Greater(maxValue, 100);
            }
        }
    }
}

