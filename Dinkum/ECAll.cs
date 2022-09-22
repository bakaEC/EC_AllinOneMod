using System;
using System.Collections.Generic;
using System.Linq;

using System.Reflection;
using BepInEx;
using UnityEngine;
using HarmonyLib;
using BepInEx.Configuration;
using BepInEx.Core.Logging.Interpolation;
using BepInEx.Logging;

namespace EC_AllInOne
{
    [BepInPlugin("cc.bakae.plugin", "ec_allinone", "0.0.2")]
    public class AutoSave: BaseUnityPlugin
    {
        private Harmony _harmony;
		public static float _newTimer = 0.45f;
		public static float _radius = 3.5f;
		int count = 0;
		public static bool _struggle = false;
		public static bool _skipMinigame = false;
		bool fish_running = false;


		ConfigEntry<KeyCode> _SavehotKey;
		ConfigEntry<KeyCode> _MailhotKey;
		ConfigEntry<KeyCode> _FishhotKey;

		void Start()
        {
			
			_SavehotKey = base.Config.Bind<KeyCode>("config", "save", KeyCode.N, "Manual Save Hotkey");
			_MailhotKey = base.Config.Bind<KeyCode>("config", "mail", KeyCode.L, "Mail grabber Hotkey");
			_FishhotKey = base.Config.Bind<KeyCode>("config", "fish", KeyCode.F10, "Fish helper Hotkey");

			ManualLogSource logger = base.Logger;
			bool flag=false;
			BepInExInfoLogInterpolatedStringHandler bepInExInfoLogInterpolatedStringHandler = new BepInExInfoLogInterpolatedStringHandler(18, 1, out flag);
			if (flag)
			{
				bepInExInfoLogInterpolatedStringHandler.AppendLiteral("Plugin ");
				bepInExInfoLogInterpolatedStringHandler.AppendFormatted<string>("cc.bakae.plugin.Allinone");
				bepInExInfoLogInterpolatedStringHandler.AppendLiteral(" is loaded!");
			}
			logger.LogInfo(bepInExInfoLogInterpolatedStringHandler);

		}

		void Update()
		{

			if (Input.GetKeyDown(this._MailhotKey.Value))
			{
				NotificationManager.manage.createChatNotification("Mail Recieving");
				/*MailManager.manage.openMailWindow();*/

				while (count < 20)
				{
					Logger.LogInfo(count++);
					/*MailManager.manage.showLetter(count);
					*/
					MailManager.manage.openMailWindow();
					try
					{
						MailManager.manage.showLetter(1);
						MailManager.manage.takeAttachment();
						MailManager.manage.closeMailWindow();

					}
					catch (Exception ex) { Logger.LogInfo("getattach fail"); }
					try
					{
						MailManager.manage.showLetter(1);
						MailManager.manage.deleteButton();
						MailManager.manage.closeMailWindow();

					}
					catch (Exception ex) { Logger.LogInfo("delete fail"); continue; }

				}
			}
			else if (Input.GetKeyUp(this._MailhotKey.Value))
			{
				try
				{
					MailManager.manage.showLetter(0);
					MailManager.manage.takeAttachment();
					MailManager.manage.closeMailWindow();

				}
				catch (Exception ex) { Logger.LogInfo("getattach fail"); }
				try
				{
					MailManager.manage.showLetter(0);
					MailManager.manage.deleteButton();
					MailManager.manage.closeMailWindow();

				}
				catch (Exception ex) { Logger.LogInfo("delete fail");}
				count = 0;
				MailManager.manage.closeShowLetterWindow();
				MailManager.manage.closeMailWindow();
				NotificationManager.manage.makeTopNotification("Rewards Got! Mails Deleted!");
			}
			if (Input.GetKeyUp(this._SavehotKey.Value))
			{
				NotificationManager.manage.createChatNotification("Mannual saving...");
				NetworkPlayersManager.manage.saveButton();
			}
			

			if (Input.GetKeyDown(this._FishhotKey.Value)&&fish_running==false)
			{
				NotificationManager.manage.createChatNotification("Easyfish ON");
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
				
			}else if(Input.GetKeyDown(this._FishhotKey.Value) && fish_running == true)
            {
				_harmony.UnpatchSelf();
				NotificationManager.manage.createChatNotification("Easyfish OFF");
				AnimalManager.manage.fishBookOpen = false;
				AnimalManager.manage.lookAtFishBook.Invoke();
				fish_running =false;
			}
			
		}
		

	}
        
    }
    class Patches{

		private static void DebugInstructions(IList<CodeInstruction> instructions)
		{
			
		}

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

		private static bool FishStrugglePrefix()
		{
			return EC_AllInOne.AutoSave._struggle;
		}


		private static void SpawnFishDummyPostfix(FishingRodCastAndReel __instance)
		{
			if (!EC_AllInOne.AutoSave._skipMinigame)
			{
				return;
			}
			__instance.networkVersion.CmdCompleteReel();
		}
	
}


