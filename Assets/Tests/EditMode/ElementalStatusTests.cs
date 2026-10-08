using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace WordVenture.Tests
{
    public sealed class ElementalStatusTests
    {
        [Test]
        public void 실제_슬라임과_상태_오버레이_미리보기를_렌더링한다()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
                Assert.Ignore("이미지 렌더링에는 그래픽 장치가 필요하다.");
            ElementalStatusPreview.Render();
        }

        static Type RuntimeType(string name) => AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType(name)).First(t => t != null);
        static object State() => Activator.CreateInstance(RuntimeType("Combat.ElementalStatus"));
        static object State(string defense) => Activator.CreateInstance(RuntimeType("Combat.ElementalStatus"), new[] { Magic(defense) });
        static object Magic(string name) => Enum.Parse(RuntimeType("Cards.MagicType"), name);
        static object Call(object state, string name, params object[] args) => state.GetType().GetMethod(name).Invoke(state, args);
        static T Get<T>(object value, string name) => (T)value.GetType().GetProperty(name).GetValue(value);
        static T Field<T>(object value, string name) => (T)value.GetType().GetField(name).GetValue(value);
        static object Hit(object state, string element, string spell = "Shoot", float damage = 20f, float affinity = 1f, bool boss = false)
            => Call(state, "Hit", Magic(element), Magic(spell), damage, affinity, boss);

        [Test]
        public void 불_적은_화상을_줄이고_기존_약점과_저항은_화상에도_적용한다()
        {
            object fire = State("Fire"); Hit(fire, "Fire", damage: 40f);
            Assert.That(Get<int>(fire, "BurnDamage"), Is.EqualTo(4));
            object ice = State("Ice"); Hit(ice, "Fire", damage: 40f, affinity: 1.5f);
            Assert.That(Get<int>(ice, "BurnDamage"), Is.EqualTo(12));
            object undead = State("Undead"); Hit(undead, "Fire", damage: 40f, affinity: 0.7f);
            Assert.That(Get<int>(undead, "BurnDamage"), Is.EqualTo(5));
        }

        [Test]
        public void 얼음_적은_냉기_6점에서_빙결된다()
        {
            object s = State("Ice"); Hit(s, "Ice"); Hit(s, "Ice");
            Assert.That(Get<int>(s, "RequiredChill"), Is.EqualTo(6));
            Assert.That(Get<bool>(s, "Frozen"), Is.False);
            Hit(s, "Ice");
            Assert.That(Get<bool>(s, "Frozen"), Is.True);
        }

        [Test]
        public void 번개_적의_감전_증폭은_절반이다()
        {
            object s = State("Lightning"); Hit(s, "Lightning");
            Assert.That(Field<int>(Hit(s, "Holy", damage: 40f), "Damage"), Is.EqualTo(45));
        }

        [Test]
        public void 바위_적은_균열로_저항이_절반만_해제된다()
        {
            object s = State("Rock"); Hit(s, "Rock");
            Assert.That(Field<int>(Hit(s, "Holy", damage: 40f, affinity: 0.5f), "Damage"), Is.EqualTo(30));
            Hit(s, "Rock");
            Assert.That(Field<int>(Hit(s, "Holy", damage: 40f, affinity: 1.5f), "Damage"), Is.EqualTo(60));
        }

        [Test]
        public void 화상은_적_턴_끝에_두번_발동하고_만료한다()
        {
            object s = State();
            Hit(s, "Fire");
            Assert.That(Call(s, "EndEnemyTurn"), Is.EqualTo(4));
            Assert.That(Call(s, "EndEnemyTurn"), Is.EqualTo(4));
            Assert.That(Call(s, "EndEnemyTurn"), Is.EqualTo(0));
            Hit(s, "Fire", "Drop");
            Assert.That(Get<int>(s, "BurnTurns"), Is.EqualTo(1));
        }

        [TestCase("Shoot", 2)]
        [TestCase("Drop", 1)]
        [TestCase("Explode", 1)]
        public void 냉기_누적은_공격_형식에_따라_다르다(string spell, int chill)
        {
            object s = State(); Hit(s, "Ice", spell);
            Assert.That(Get<int>(s, "Chill"), Is.EqualTo(chill));
            Assert.That(Get<float>(s, "MovementMultiplier"), Is.EqualTo(0.5f));
        }

        [Test]
        public void 빙결은_한번_행동을_막고_다음_행동까지_재빙결을_막는다()
        {
            object s = State(); Hit(s, "Ice"); Hit(s, "Ice", "Drop");
            Assert.That(Get<bool>(s, "Frozen"), Is.True);
            Assert.That(Call(s, "BeginEnemyTurn"), Is.EqualTo(true));
            Call(s, "EndEnemyTurn");
            Assert.That(Get<bool>(s, "Frozen"), Is.False);
            Hit(s, "Ice"); Hit(s, "Ice");
            Assert.That(Get<bool>(s, "Frozen"), Is.False);
            Assert.That(Call(s, "BeginEnemyTurn"), Is.EqualTo(false));
            Call(s, "EndEnemyTurn"); Hit(s, "Ice", "Drop");
            Assert.That(Get<bool>(s, "Frozen"), Is.True);
        }

        [Test]
        public void 보스는_빙결_대신_다음_공격이_약해진다()
        {
            object s = State(); Hit(s, "Ice", boss: true); Hit(s, "Ice", boss: true);
            Assert.That(Get<bool>(s, "Frozen"), Is.False);
            Assert.That(Call(s, "BeginEnemyTurn"), Is.EqualTo(false));
            Assert.That(Call(s, "GetAttackDamage", 10), Is.EqualTo(7));
            Call(s, "EndEnemyTurn");
            Assert.That(Get<bool>(s, "Weakened"), Is.False);
        }

        [TestCase("Fire", "Lightning", "불꽃 방전", 8)]
        [TestCase("Ice", "Rock", "쇄빙", 10)]
        [TestCase("Lightning", "Ice", "신경 마비", 0)]
        [TestCase("Rock", "Fire", "용암 균열", 0)]
        public void 네_연계가_확정으로_발동한다(string first, string second, string reaction, int extra)
        {
            object s = State(); Hit(s, first); object result = Hit(s, second);
            Assert.That(Field<string>(result, "Reaction"), Is.EqualTo(reaction));
            Assert.That(Field<int>(result, "ExtraDamage"), Is.EqualTo(extra));
            if (first == "Fire") Assert.That(Get<int>(s, "BurnTurns"), Is.Zero);
            if (first == "Ice") Assert.That(Get<int>(s, "Chill"), Is.Zero);
            if (first == "Lightning") Assert.That(Get<bool>(s, "Weakened"), Is.True);
            if (first == "Rock") Assert.That(Get<int>(s, "BurnTurns"), Is.EqualTo(3));
        }

        [Test]
        public void 감전과_균열은_한번_소비하고_약점_보너스는_유지한다()
        {
            object s = State(); Hit(s, "Lightning");
            object result = Hit(s, "Holy");
            Assert.That(Field<int>(result, "Damage"), Is.EqualTo(25));
            Assert.That(Get<int>(s, "ShockTurns"), Is.Zero);
            Assert.That(Get<int>(s, "FractureTurns"), Is.Zero);
            Assert.That(Field<int>(Hit(s, "Holy", affinity: 0.7f), "Damage"), Is.EqualTo(14));
            Hit(s, "Rock");
            Assert.That(Field<int>(Hit(s, "Holy", affinity: 0.7f), "Damage"), Is.EqualTo(20));
            Hit(s, "Rock");
            Assert.That(Field<int>(Hit(s, "Holy", affinity: 1.5f), "Damage"), Is.EqualTo(30));
        }

        [Test]
        public void 화상과_냉기는_서로_지워지고_약한_화상은_피해를_낮추지_않는다()
        {
            object s = State(); Hit(s, "Fire"); Hit(s, "Fire", "Drop", damage: 5f);
            Assert.That(Get<int>(s, "BurnDamage"), Is.EqualTo(4));
            Assert.That(Get<int>(s, "BurnTurns"), Is.EqualTo(2));
            Assert.That(Field<bool>(Hit(s, "Ice"), "RemovesBurn"), Is.True);
            Assert.That(Get<int>(s, "BurnTurns"), Is.Zero);
            Assert.That(Field<bool>(Hit(s, "Fire"), "RemovesChill"), Is.True);
            Assert.That(Get<int>(s, "Chill"), Is.Zero);
        }

        [Test]
        public void 미리보기는_실제_상태를_바꾸지_않으며_회복은_상태를_소비하지_않는다()
        {
            object s = State(); Hit(s, "Fire");
            object preview = Call(s, "Preview", Magic("Lightning"), Magic("Shoot"), 20f, 1f, false);
            Assert.That(Field<string>(preview, "Reaction"), Is.EqualTo("불꽃 방전"));
            Assert.That(Get<int>(s, "BurnTurns"), Is.EqualTo(2));
            Hit(s, "Lightning"); Hit(s, "Rock");
            object heal = Hit(s, "Holy", affinity: -1f);
            Assert.That(Field<int>(heal, "Damage"), Is.EqualTo(-20));
            Assert.That(Get<int>(s, "FractureTurns"), Is.EqualTo(2));
        }

        [Test]
        public void 사용하지_않은_상태도_두_적_턴_후_만료한다()
        {
            object s = State(); Hit(s, "Ice"); Hit(s, "Lightning"); Hit(s, "Rock");
            Call(s, "EndEnemyTurn"); Call(s, "EndEnemyTurn");
            Assert.That(Get<int>(s, "Chill"), Is.Zero);
            Assert.That(Get<int>(s, "ShockTurns"), Is.Zero);
            Assert.That(Get<int>(s, "FractureTurns"), Is.Zero);
        }

        [Test]
        public void 새_전투_준비는_적의_상태를_초기화한다()
        {
            GameObject go = new GameObject("StatusResetTest");
            try
            {
                Component enemy = go.AddComponent(RuntimeType("Combat.Enemies.Enemy"));
                object s = Get<object>(enemy, "Status"); Hit(s, "Fire"); Hit(s, "Lightning");
                object data = Activator.CreateInstance(RuntimeType("Combat.Enemies.EnemyData"));
                data.GetType().GetField("maxHp").SetValue(data, 30);
                Call(enemy, "InitEnemyData", data);
                s = Get<object>(enemy, "Status");
                Assert.That(Get<int>(s, "BurnTurns"), Is.Zero);
                Assert.That(Get<int>(s, "ShockTurns"), Is.Zero);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void 생성_시트는_투명하고_슬라임_본체보다_앞에_겹쳐_그린다()
        {
            Texture2D atlas = Resources.Load<Texture2D>("Combat/ElementalStatusOverlay");
            Assert.That(atlas, Is.Not.Null);
            Assert.That(atlas.filterMode, Is.EqualTo(FilterMode.Point));
            Assert.That(atlas.GetPixels32().Count(p => p.a == 0), Is.GreaterThan(atlas.width * atlas.height / 4));
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/Enemies/Slime/BossCactus.prefab");
            GameObject instance = Object.Instantiate(prefab);
            try
            {
                Component enemy = instance.GetComponent(RuntimeType("Combat.Enemies.Enemy"));
                Component vfx = instance.GetComponent(RuntimeType("Combat.Enemies.ElementalStatusVfx"));
                if (vfx == null)
                {
                    enemy.GetType().GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance)
                        ?.Invoke(enemy, null);
                    // Awake가 기반 클래스의 private 메서드라 하위 클래스에서 직접 찾을 수 없다.
                    if (instance.GetComponent(RuntimeType("Combat.Enemies.ElementalStatusVfx")) == null)
                        RuntimeType("Combat.Enemies.Enemy").GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance)
                            .Invoke(enemy, null);
                    vfx = instance.GetComponent(RuntimeType("Combat.Enemies.ElementalStatusVfx"));
                }
                SpriteRenderer body = instance.GetComponent<SpriteRenderer>();
                object s = Get<object>(enemy, "Status"); Hit(s, "Fire");
                vfx.GetType().GetMethod("LateUpdate", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(vfx, null);
                SpriteRenderer overlay = instance.transform.Find("StatusOverlay0").GetComponent<SpriteRenderer>();
                Assert.That(overlay.sortingLayerID, Is.EqualTo(body.sortingLayerID));
                Assert.That(overlay.sortingOrder, Is.GreaterThan(body.sortingOrder));
                Bounds bounds = (Bounds)RuntimeType("Combat.Enemies.ElementalStatusVfx")
                    .GetMethod("GetVisibleBodyBounds").Invoke(null, new object[] { body.sprite });
                Vector2[] visibleSizes = (Vector2[])RuntimeType("Combat.Enemies.ElementalStatusVfx")
                    .GetField("visibleSizes", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
                float effectBottom = overlay.transform.localPosition.y
                    - visibleSizes[0].y * overlay.transform.localScale.y * 0.5f;
                Assert.That(effectBottom, Is.EqualTo(bounds.min.y).Within(0.0001f), "확대해도 이펙트 아래 경계는 발밑에 맞춘다");
                Assert.That(overlay.sprite, Is.Not.Null);
            }
            finally { Object.DestroyImmediate(instance); }
        }
    }
}
