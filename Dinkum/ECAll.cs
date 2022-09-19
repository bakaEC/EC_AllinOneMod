using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Security;
using UnityEngine.Bindings;
using UnityEngine.Internal;
using UnityEngine.Scripting;
using UnityEngineInternal;
using System.Reflection;
using BepInEx;
using UnityEngine;
using UnityEngine.Events;
using HarmonyLib;

namespace EC_AllInOne
{
    [BepInPlugin("cc.bakae.plugin", "ec_allinone", "0.0.1")]
    public class AutoSave: BaseUnityPlugin
    {
        private Harmony _harmony;
		public static float _newTimer = 0.45f;
		public static float _radius = 3.5f;
		public static bool _struggle = false;
		public static bool _skipMinigame = false;
		bool fish_running = false;
		bool fish_seek = false;

		void Start()
        {

            Logger.LogInfo("EC active!");
            
        }

		void Update()
		{

			if (Input.GetKeyDown(KeyCode.F9)){
				
			}
			if (Input.GetKeyDown(KeyCode.F11))
			{
				NotificationManager.manage.createChatNotification("手动保存中!");
				NetworkPlayersManager.manage.saveButton();
			}
			

			if (Input.GetKeyDown(KeyCode.F10)&&fish_running==false)
			{
				NotificationManager.manage.createChatNotification("开心钓鱼模式开启");
				StatusManager.manage.addBuff(StatusManager.BuffType.fishingBuff, 1000, 1);
				_harmony = new Harmony("easyfishing");
				_harmony.PatchAll();
				MethodInfo methodInfo = AccessTools.EnumeratorMoveNext(AccessTools.Method(typeof(NetworkFishingRod), "onLocalBiteTimer", null, null));
				MethodInfo methodInfo2 = AccessTools.Method(typeof(Patches), "OnLocalBiteTimerTranspiler", null, null);
				MethodInfo methodInfo3 = AccessTools.EnumeratorMoveNext(AccessTools.Method(typeof(FishScript), "checkForLure", null, null));
				MethodInfo methodInfo4 = AccessTools.Method(typeof(Patches), "CheckForLureTranspiler", null, null);
				MethodInfo methodInfo5 = AccessTools.EnumeratorMoveNext(AccessTools.Method(typeof(NetworkFishingRod), "fishStruggle", null, null));
				MethodInfo methodInfo6 = AccessTools.Method(typeof(Patches), "FishStrugglePrefix", null, null);
				MethodInfo methodInfo7 = AccessTools.Method(typeof(FishingRodCastAndReel), "spawnFishDummy", null, null);
				MethodInfo methodInfo8 = AccessTools.Method(typeof(Patches), "SpawnFishDummyPostfix", null, null);
				_harmony.Patch(methodInfo, null, null, new HarmonyMethod(methodInfo2), null, null);
				_harmony.Patch(methodInfo3, null, null, new HarmonyMethod(methodInfo4), null, null);
				_harmony.Patch(methodInfo5, new HarmonyMethod(methodInfo6), null, null, null, null);
				_harmony.Patch(methodInfo7, null, new HarmonyMethod(methodInfo8), null, null, null);
				AnimalManager.manage.fishBookOpen = true;
				AnimalManager.manage.lookAtFishBook.Invoke();
				fish_running = true;
			}else if(Input.GetKeyDown(KeyCode.F10) && fish_running == true)
            {
				_harmony.UnpatchSelf();
				NotificationManager.manage.createChatNotification("开心钓鱼模式关闭");
				AnimalManager.manage.fishBookOpen = false;
				AnimalManager.manage.lookAtFishBook.Invoke();
				fish_running =false;
			}
			/*if (Input.GetKeyDown(KeyCode.F9)&&fish_seek==false)
			{
				AnimalManager.manage.fishBookOpen = true;
				AnimalManager.manage.lookAtFishBook.Invoke();
				fish_seek = true;
			}else if (Input.GetKeyDown(KeyCode.F9) && fish_seek == true)
            {
				AnimalManager.manage.fishBookOpen = false;
				AnimalManager.manage.lookAtFishBook.Invoke();
				fish_seek=false;
            }*/
		}

	}
        
    }
    class Patches{
		// Token: 0x06000003 RID: 3 RVA: 0x00002234 File Offset: 0x00000434
		private static void DebugInstructions(IList<CodeInstruction> instructions)
		{
			
		}

		// Token: 0x06000004 RID: 4 RVA: 0x0000228C File Offset: 0x0000048C
		private static int? FindPattern(IList<CodeInstruction> instructions, IReadOnlyCollection<string> pattern)
		{
			int? result = null;
			int i;
            Func<string, int, bool> nine=null;
			int k;
			for (i = 0; i < instructions.Count - pattern.Count + 1; i = k + 1)
			{
				Func<string, int, bool> func;
				if ((func = nine) == null)
				{
					func = (nine = ((string t, int j) => instructions[i + j].opcode.Name != t));
				}
				if (!Enumerable.Any<string>(Enumerable.Where<string>(pattern, func)))
				{
					result = new int?(i);
					break;
				}
				k = i;
			}
			return result;
		}

		// Token: 0x06000005 RID: 5 RVA: 0x00002320 File Offset: 0x00000520
		private static IEnumerable<CodeInstruction> OnLocalBiteTimerTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> list = Enumerable.ToList<CodeInstruction>(instructions);
			string[] pattern = new string[]
			{
					"ldarg.0",
					"ldfld",
					"ldc.r4",
					"ble",
					"ldloc.1",
					"call",
					"ldc.i4.0",
					"ret"
			};
			int? num = Patches.FindPattern(list, pattern);
			if (num == null)
			{
				
				return list;
			}
			
			list[num.Value + 2].operand = EC_AllInOne.AutoSave._newTimer;
			return list;
		}

		private static IEnumerable<CodeInstruction> CheckForLureTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> list = Enumerable.ToList<CodeInstruction>(instructions);
			string[] pattern = new string[]
			{
					"ldloc.2",
					"ldc.r4",
					"ldloc.1",
					"ldfld",
					"call",
					"call",
					"brfalse",
					"ldloc.2",
					"ldc.r4",
					"ldloc.1",
					"ldfld",
					"call",
					"call"
			};
			int? num = Patches.FindPattern(list, pattern);
			if (num == null)
			{
				return list;
			}

			list[num.Value + 1].operand = EC_AllInOne.AutoSave._radius;
			list[num.Value + 8].operand = EC_AllInOne.AutoSave._radius;
			return list;
		}

		// Token: 0x06000007 RID: 7 RVA: 0x000024F8 File Offset: 0x000006F8
		private static bool FishStrugglePrefix()
		{
			return EC_AllInOne.AutoSave._struggle;
		}

		// Token: 0x06000008 RID: 8 RVA: 0x00002504 File Offset: 0x00000704
		private static void SpawnFishDummyPostfix(FishingRodCastAndReel __instance)
		{
			if (!EC_AllInOne.AutoSave._skipMinigame)
			{
				return;
			}
			__instance.networkVersion.CmdCompleteReel();
		}
	}

