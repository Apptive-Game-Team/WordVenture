#if ARTEL_GAME_FUNCTIONS && (UNITY_EDITOR || DEVELOPMENT_BUILD)
using System;
using System.Collections;
using System.Linq;
using Artel;
using Combat.Enemies;
using Combat.Stage;
using Map;
using Scenes;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
    public static class ArtelTestCheats
    {
        static bool transitioning;
        static bool clearingEnemies;

        [ArtelCheat("현재 전투의 살아 있는 모든 적에게 지정한 직접 피해를 준다. 속성/방패 계산 없이 TakeHit을 사용하며 사망 효과는 유지한다. 피해 검증 TC에는 사용하지 않는다.",
            name: "damage_all_enemies", when: nameof(CanAffectEnemies))]
        public static string DamageAllEnemies([CheatRange(1, 1000000)] int damage)
        {
            if (damage < 1 || damage > 1000000) throw new ArgumentOutOfRangeException(nameof(damage));
            Require(CanAffectEnemies());
            int affected = 0;
            // 사망 연쇄로 이미 죽은 적은 중복 타격하지 않고, 새로 분열한 적에게도 재차 피해를 주지 않는다.
            foreach (Enemy enemy in LiveEnemies())
            {
                if (enemy == null || !enemy.IsAlive) continue;
                enemy.TakeHit(damage);
                affected++;
            }
            return "affected=" + affected + " damage=" + damage;
        }

        [ArtelCheat("현재 웨이브의 살아 있는 적과 사망 시 분열한 자식을 Kill로 쓰러뜨린다. 다음 웨이브는 건드리지 않으며 정상 사망/클리어 처리를 기다린다. 처치 검증 TC에는 사용하지 않는다.",
            name: "kill_all_enemies", when: nameof(CanAffectEnemies))]
        public static IEnumerator KillAllEnemies()
        {
            Require(CanAffectEnemies());
            Scene battleScene = SceneManager.GetActiveScene();
            clearingEnemies = true;
            int killed = 0;
            try
            {
                while (true)
                {
                    if (SceneManager.GetActiveScene() != battleScene)
                        throw new InvalidOperationException("Battle scene changed during the cheat.");
                    Enemy[] enemies = LiveEnemies();
                    if (enemies.Length == 0) break;
                    // 새로 소환된 분열 자식의 Start(Animator 초기화)가 실행된 뒤 처치한다.
                    yield return null;
                    if (SceneManager.GetActiveScene() != battleScene)
                        throw new InvalidOperationException("Battle scene changed during the cheat.");
                    foreach (Enemy enemy in enemies)
                    {
                        if (enemy == null || !enemy.IsAlive) continue;
                        if (killed >= 1024) throw new InvalidOperationException("Enemy spawn chain exceeded 1024 kills.");
                        enemy.Kill();
                        killed++;
                    }
                    // 살아 있는 적이 없어지면 즉시 끝낸다. 다음 웨이브를 기다리며 반복하지 않는다.
                }
            }
            finally { clearingEnemies = false; }
            yield return "killed=" + killed;
        }

        static Enemy[] LiveEnemies() => UnityEngine.Object.FindObjectsOfType<Enemy>()
            .Where(enemy => enemy.IsAlive && enemy.gameObject.scene == SceneManager.GetActiveScene()).ToArray();

        static string CanAffectEnemies()
        {
            if (transitioning || clearingEnemies) return "Another cheat is in progress.";
            if (SceneManager.GetActiveScene().name != "TurnBattleScene") return "Not in a battle scene.";
            if (InteractionLock.IsLocked) return "Close menus and finish dialogue first.";
            return LiveEnemies().Length == 0 ? "No living enemies in the current wave." : null;
        }

        [ArtelCheat("타이틀에서 로컬 테스트 저장을 덮어쓴다. 진행도 -1=새 저장, 0~9=다음 스테이지, 10=전체 완료. 기존 대화/도감 기록을 지운다. 서버 저장은 변경하지 않는다.",
            name: "prepare_test_save", when: nameof(CanPrepareSave))]
        public static string PrepareTestSave([CheatRange(-1, 10)] int progress,
            bool confirmOverwrite = false, bool tutorialEnded = true, bool showActOneMap = false)
        {
            if (progress < -1 || progress > 10) throw new ArgumentOutOfRangeException(nameof(progress));
            if (!confirmOverwrite) throw new InvalidOperationException("Set confirmOverwrite=true to replace the local test save.");
            if (showActOneMap && progress < 5)
                throw new ArgumentException("Act one map selection requires progress >= 5.", nameof(showActOneMap));
            Require(CanPrepareSave());

            SaveLoadController.Instance.InitPlayData();
            MapMove.StagePosition = progress;
            SaveLoadController.Instance.SavePlayData();
            if (progress >= 0 && tutorialEnded) SaveLoadController.MarkTutorialEnded();
            SaveLoadController.ShowsActOneMap = showActOneMap;
            PlayerPrefs.Save();
            UnityEngine.Object.FindObjectOfType<TitleSceneManager>().ShowSavedProgress();
            return "Prepared local test save: progress=" + progress + ", tutorialEnded=" + SaveLoadController.IsTutorialEnded;
        }

        static string CanPrepareSave()
        {
            if (transitioning || clearingEnemies) return "Another cheat is in progress.";
            if (SceneManager.GetActiveScene().name != "TitleScene") return "Return to TitleScene first.";
            if (SaveLoadController.Instance == null || UnityEngine.Object.FindObjectOfType<TitleSceneManager>() == null)
                return "Title scene has not finished initializing.";
            return null;
        }

        [ArtelCheat("맵에서 해금된 스테이지(0~9)의 전투로 바로 이동한다. 맵 진입 대화를 건너뛰므로 해당 대화 검증에는 쓰지 않는다. 진행도는 올리지 않는다.",
            name: "enter_test_battle", when: nameof(CanEnterBattle))]
        public static IEnumerator EnterTestBattle([CheatRange(0, 9)] int stage)
        {
            if (stage < 0 || stage > 9) throw new ArgumentOutOfRangeException(nameof(stage));
            Require(CanEnterBattle());
            if (stage > MapMove.StagePosition) throw new InvalidOperationException("Stage is locked. Prepare the test save first.");
            StageDataSingleton.Instance.stagePosition = stage;
            StageDataSingleton.Instance.isFirstClear = false;
            yield return TransitionTo("TurnBattleScene");
        }

        static string CanEnterBattle()
        {
            if (transitioning || clearingEnemies) return "Another cheat is in progress.";
            if (SceneManager.GetActiveScene().name != "MapScene" || StageDataSingleton.Instance == null)
                return "Open MapScene and wait for initialization first.";
            if (InteractionLock.IsLocked) return "Close menus and finish dialogue first.";
            return null;
        }

        [ArtelCheat("현재 진행도를 로컬에 저장하고 타이틀로 돌아간다. 전투 승리나 클리어 보상은 주지 않는다.",
            name: "return_to_test_title", when: nameof(CanReturnToTitle))]
        public static IEnumerator ReturnToTestTitle()
        {
            Require(CanReturnToTitle());
            SaveLoadController.Instance.SavePlayData();
            PlayerPrefs.Save();
            yield return TransitionTo("TitleScene");
        }

        static string CanReturnToTitle() => transitioning || clearingEnemies ? "Another cheat is in progress."
            : SaveLoadController.Instance == null ? "Save controller is not ready." : null;

        static IEnumerator TransitionTo(string scene)
        {
            transitioning = true;
            try
            {
                if (SettingsMenu.Instance != null) SettingsMenu.Instance.Close();
                Time.timeScale = 1;
                yield return SceneManager.LoadSceneAsync(scene);
                // 이전 씬의 대화/도감 잠금은 타이틀로 가져오지 않는다. 전투의 새 대화 잠금은 보존한다.
                if (scene == "TitleScene")
                {
                    InteractionLock.IsLocked = false;
                    InteractionLock.IsPaused = false;
                }
                yield return null;
            }
            finally { transitioning = false; }
        }

        static void Require(string reason)
        {
            if (reason != null) throw new InvalidOperationException(reason);
        }
    }
}
#endif
