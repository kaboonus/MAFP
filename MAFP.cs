/*Mining  Anomaly and Fleet +  protection( bonus)
**in developement, he has some unused voids and bugs etc etc so, do not cry too much if is not working at 100%
this script will take mining anomalies, keep distance to fleet leader (usual an rorqual) and  deposit the ore into his fleet hangar
he can use drones , btw ( you have to activate lines in INBELTSITE)
if rats are on grid and  he will broadcast in fleet window " enemy spotted", even if he will warp home to keep him alive,
IF FLEET MATE IS IN SYSTEM
but safe is to use him in your own fleet 
+BONUS:
at the end of file you find the support ( you can add on garbage/only commander/marvel ) for warping to you miner ,
so your "protector" could  do his anomalies ( and yes this is working)
* used  on ice, but is designed  ( and not tested, because i cannot minewith  crystals) to change the crystal in function of ore
 */

using BotSharp.ToScript.Extension;
using Parse = Sanderling.Parse;
using MemoryStruct = Sanderling.Interface.MemoryStruct;
using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;
//to change faster if you need
bool DepositOre = false;
bool UseToAllarm = false;
//	begin of configuration section ->
string VersionScript = "M.A.F.P v1.1";//do not change
//	Bookmark of location where ore should be unloaded.
var SecurityLevel =  Measurement?.InfoPanelCurrentSystem?.SecurityLevelMilli;
Host.Log( "Security Level: " +SecurityLevel/1000);
string UnloadBookmark = "home";
string IgnoreNeutral = "xxxxx|wwwww"; //
//	Name of the container to unload to as shown in inventory.
string UnloadDestContainerName = "Item Hangar";
string FollowChiefName = "name of alt with rorqual";
string UnloadDestFleetContainer = "Rorqual (Fleet Hangar)";
string FollowShipType = "rorqual";
//	when this is set to true, the bot will try to unload when undocked.
string FleetMate = "name of alt who protect you";

//	Bookmark of place to retreat to to prevent ship loss.
string RetreatBookmark = UnloadBookmark;
bool takeateroids = true;
string FolderAsteroids = "asteroid belts";
//	The bot loads this preset to the active tab. 
var minutesToDT = 10;
var hoursToDT = 0;
var hoursToSession = 23;
var minutesToSession = 11;

var ActivateHardener = true; // activate shield hardener.

//	bot will start fighting (and stop mining) when hitpoints are lower. 
var DefenseEnterHitpointThresholdPercent = 85;
var DefenseExitHitpointThresholdPercent = 90;

var EmergencyWarpOutHitpointPercent = 60;
string AnomalyToTakeColumnHeader = "name";
string AnomalyToTake = "icicle|glacial|ice|glaze";
string MiningTab = "logi";
bool RetreatOnNeutralOrHostileInLocal;
//var EnterOffloadOreHoldFillPercent = 98;	//	percentage of ore hold fill level at which to enter the offload process.
if (SecurityLevel>500)
 RetreatOnNeutralOrHostileInLocal = false;
 else 
RetreatOnNeutralOrHostileInLocal = true;   // warp to RetreatBookmark when a neutral or hostile is visible in local.
Host.Log( "Retreat on Hostiles: " +RetreatOnNeutralOrHostileInLocal);
bool returnDronesToBayOnRetreat = true; // when set to true, bot will attempt to dock back the drones before retreating
int x;
string WarpToAnomalyDistance = "Within 30 km";
Dictionary<string,int> dict=new Dictionary<string,int>();
dict.Add("Within 0 m",0);
dict.Add("Within 10 km",1);
dict.Add("Within 20 km",2);
dict.Add("Within 30 km",3);
dict.Add("Within 50 km",4);
dict.Add("Within 70 km",5);
dict.Add("Within 100 km",6);
dict.TryGetValue(WarpToAnomalyDistance,out x);


bool GoToUnload;
bool UseMiningCrystal;
string[] BaseOre = new[] {
	"Dark Glitter", "Clear Icicle", "Gelidus",
	"Mercoxit", "Bistot", "Arkonor",
	"Crokite", "Spodumain", "Ochre",
	"Gneiss", "Hedbergite", "Hemorphite",
	"Jaspet", "Kernite", "Omber",
	"Plagioclase", "Pyroxeres",
	"Scordite", "Veldspar"
	
	};
string[] ModifierOrePreference = new[] {
	"Crust", "Glitter", "Icicle", "Gelidus", "Blue Ice",
	"Vitreous Mercoxit", "Magma Mercoxit", "Cubic Bistot",
	"Monoclinic Bistot", "Triclinic Bistot", "Flawless Arkonor",
	"Prime Arkonor", "Crimson Arkonor", "Pellucid Crokite", "Crystalline Crokite",
	"Sharp Crokite", "Dazzling Spodumain", "Gleaming Spodumain", "Bright Spodumain",
	"Jet Ochre", "Obsidian Ochre", "Onyx Ochre", "Brilliant Gneiss", "Prismatic Gneiss",
	"Iridescent Gneiss", "Lustrous Hedbergite", "Glazed Hedbergite", "Vitric Hedbergite",
	"Scintillating Hemorphite", "Radiant Hemorphite", "Vivid Hemorphite",
	"Immaculate Jaspet", "Pristine Jaspet", "Pure Jaspet", "Resplendant Kernite",
	"Fiery Kernite", "Luminous Kernite", "Platinoid Omber", "Golden Omber",
	"Silvery Omber", "Sparkling Plagioclase", "Rich Plagioclase",
	"Azure Plagioclase", "Opulent Pyroxeres", "Viscous Pyroxeres",
	"Solid Pyroxeres", "Glossy Scordite", "Massive Scordite",
	"Condensed Scordite", "Stable Veldspar", "Dense Veldspar", "Concentrated Veldspar",


	};
string [] ModuleWithCrystal = new [] {
"Modulated Deep Core Miner II", "Modulated Deep Core Miner II", "Modulated Strip Miner II",



};





// Host.Log( " Drones name : " +LabelNameMiningDrones+ "");

//keys
var lockTargetKeyCode = VirtualKeyCode.LCONTROL;
var targetLockedKeyCode = VirtualKeyCode.SHIFT;
var orbitKeyCode = VirtualKeyCode.VK_W;
var attackDrones = VirtualKeyCode.VK_F;
var returnkey = VirtualKeyCode.RETURN;
var spacekey = VirtualKeyCode.SPACE;
var Warpkey = VirtualKeyCode.VK_S;
var Dockkey = VirtualKeyCode.VK_D;
var startSession = DateTime.Now;
var playSession = DateTime.UtcNow.AddHours(hoursToSession).AddMinutes(minutesToSession);
var dateAndTime = DateTime.UtcNow;
var date = dateAndTime.Date;
var eveRealServerDT =date.AddHours(11).AddMinutes(-1);
if (eveRealServerDT < dateAndTime)
{
eveRealServerDT = eveRealServerDT.AddDays(1);
Host.Log(" >  eveRealServerDT :  " +eveRealServerDT.ToString(" dd/MM/yyyy HH:mm:ss")+ " .");
}
var eveSafeDT = eveRealServerDT.AddHours(-hoursToDT).AddMinutes(-minutesToDT);
Host.Log(" >  eveSafeDT : " +eveSafeDT.ToString(" dd/MM/yyyy HH:mm")+ " .");
var FinishedMining = false;



Func<object> BotStopActivity = () => null;

Func<object> NextActivity = MainStep;

for(;;)
{
	MemoryUpdate();

	Host.Log(
		"script: " +VersionScript+
		" ;   Logout in:  "  + ((TimeSpan.FromMinutes(logoutgame) < TimeSpan.Zero) ? "-" : "") + (TimeSpan.FromMinutes(logoutgame)).ToString(@"hh\:mm\:ss")+
        
		" ;   ore hold fill: " + OreHoldFillPercent + "%" +
		" ;   mining range: " + MiningRange +
		" ;   Miner module: " + IhaveMinerModule+
		" ;   Use Crystal: " + UseMiningCrystal+
		//" ;   mining modules (inactive): " + SetModuleMiner?.Length + "(" + SetModuleMinerInactive?.Length + ")" +
		" ;   shield.hp: " + ShieldHpPercent + "%" +
		"\n" +
		
		
		" ;   Drone for mining :   " + LabelNameMiningDrones+//  "   I have them??  " +IhavegoodDrones+
		"\n" +
        " *   offload count: " + OffloadCount +
		
		" ;   overview.rats: " + ListRatOverviewEntry?.Length +
		" ;   overview.roids: " + ListAsteroidOverviewEntry?.Length +
		" ;   retreat: " + RetreatReason + 
		"\n" +
		" *   fleet protector list: " +FleetCharFlagList+
		"\n" +
		" *   nextAct: " + NextActivity?.Method?.Name);


	CloseModalUIElement();

	if(0 < RetreatReason?.Length && !(Measurement?.IsDocked ?? false) && !SpeedWarping)
	{
		if (null != WindowDrones  && returnDronesToBayOnRetreat) {
			DroneEnsureInBay();
		}

		if (!returnDronesToBayOnRetreat || (returnDronesToBayOnRetreat && 0 == DronesInSpaceCount)) {
			WarpingSlow(RetreatBookmark, "dock");
		}
		continue;
	}

	NextActivity = NextActivity?.Invoke() as Func<object>;

	if(BotStopActivity == NextActivity)
		break;
	
	if(null == NextActivity)
		NextActivity = MainStep;
	
	Host.Delay(1111);
}

//	seconds since ship was jammed.


int?	ShieldHpPercent => ShipUi?.HitpointsAndEnergy?.Shield / 10;

bool	DefenseExit =>
	(Measurement?.IsDocked ?? false) ||
	!(0 < ListRatOverviewEntry?.Length)	||
	(DefenseExitHitpointThresholdPercent < ShieldHpPercent &&
	 0 < ListRatOverviewEntry?.Length);

bool	DefenseEnter =>
	!DefenseExit	||
	!(DefenseEnterHitpointThresholdPercent < ShieldHpPercent);
int EnterOffloadOreHoldFillPercent =>  (DepositOre && IamInFleet) ? 70 : 99;
bool	OreHoldFilledForOffload => Math.Max(0, Math.Min(100, EnterOffloadOreHoldFillPercent)) <= OreHoldFillPercent;


string RetreatReasonTemporary = null;
string RetreatReasonPermanent = null;
string RetreatReason => RetreatReasonPermanent ?? RetreatReasonTemporary ;
string Icedrone => IhaveICeModule ?  "Ice Harvesting Drone" : null;
string Minedrone => IhaveMinerModule ? "Mining Drone": null;
 string LabelNameMiningDrones => Icedrone ?? Minedrone;

int? LastCheckOreHoldFillPercent = null;

int OffloadCount = 0;

Func<object>	MainStep()
{
	if(Measurement?.IsDocked ?? false)
	{
		InInventoryUnloadItems();
;
		if (logoutme || FinishedMining)
			return BotStopActivity;

		if (0 < RetreatReason?.Length)
			return MainStep;

		Undock();
	}
	
	if(DefenseEnter || GoToUnload)
	{
		Host.Log("enter defense.");
		WarpingSlow(RetreatBookmark, "dock");
	}



	EnsureWindowInventoryOpenOreHold();

	if(ReadyForManeuver)
	{
		DroneEnsureInBay();

    if(OreHoldFilledForOffload && 0 == DronesInSpaceCount && IamInFleet)
    Console.Beep(1500, 200);
		if(OreHoldFilledForOffload && 0 == DronesInSpaceCount && !IamInFleet)
		{
			if(ReadyForManeuver)
				WarpingSlow(RetreatBookmark, "dock");


			return MainStep;
		}
		
		if(!(0 < ListAsteroidOverviewEntry?.Length))
			return TakeAnomaly;
	}

	ModuleMeasureAllTooltip();

	if(ActivateHardener)
		ActivateHardenerExecute();

	return InBeltMineStep;
}


void CloseModalUIElement()
{
	var	ButtonClose =
		ModalUIElement?.ButtonText?.FirstOrDefault(button => (button?.Text).RegexMatchSuccessIgnoreCase("close|no|ok"));
		
	Sanderling.MouseClickLeft(ButtonClose);
}

void DroneLaunch()
{
	Host.Log("launch drones.");
	Sanderling.MouseClickRight(DronesInBayListEntry);
	Sanderling.MouseClickLeft(Menu?.FirstOrDefault()?.EntryFirstMatchingRegexPattern("launch", RegexOptions.IgnoreCase));
}

void DroneEnsureInBay()
{
	if(0 == DronesInSpaceCount)
		return;

	DroneReturnToBay();
	
	Host.Delay(4444);
}

void DroneReturnToBay()
{
	Host.Log("return drones to bay.");
	Sanderling.MouseClickRight(DronesInSpaceListEntry);
	Sanderling.MouseClickLeft(Menu?.FirstOrDefault()?.EntryFirstMatchingRegexPattern("return.*bay", RegexOptions.IgnoreCase));
}
int L=3;
Func<object> TakeAnomaly()
{
        Host.Log("               take Anomaly func");

    ModuleMeasureAllTooltip();

    if (miningTab != OverviewTabActive )
    {
        Sanderling.MouseClickLeft(miningTab);
        Host.Delay(311);
    }
    DroneEnsureInBay();
    

	if ( OreHoldFillPercent > 0)
    {
        Host.Log("               Cargo at : " +OreHoldFillPercent+ " %     Go to unload !");
       WarpingSlow(RetreatBookmark, "dock");;
        return MainStep;
    }

    var probeScannerWindow = Measurement?.WindowProbeScanner?.FirstOrDefault();
    var scanActuallyAnomaly = probeScannerWindow?.ScanResultView?.Entry?.FirstOrDefault(ActuallyAnomaly);
    var UndesiredAnomaly = probeScannerWindow?.ScanResultView?.Entry?.FirstOrDefault(IgnoreAnomaly);
    var scanResultCombatSite = probeScannerWindow?.ScanResultView?.Entry?.FirstOrDefault(AnomalySuitableGeneral);

    if (probeScannerWindow == null)
        FlashWindowProbes(1);
    if (null != scanActuallyAnomaly)
    {
        ClickMenuEntryOnMenuRoot(scanActuallyAnomaly, "Ignore Result");
        return TakeAnomaly;
    }
    if (null != UndesiredAnomaly)
    {
        ClickMenuEntryOnMenuRoot(UndesiredAnomaly, "Ignore Result");
        Host.Log("               working at ignoring anomalies :) be patient");
        return TakeAnomaly;
    }
    if ((null != scanResultCombatSite) && (null == UndesiredAnomaly))
    {
        Sanderling.MouseClickRight(scanResultCombatSite);
        Sanderling.WaitForMeasurement();
        var menuResult = Measurement?.Menu?.ToList();
        if (null == menuResult)
        {
            Host.Log("                R2D2 fails: not expected  menu!  ");
            return TakeAnomaly;
        }
		else
		{
            var menuResultWarp = menuResult?[0].Entry.ToArray();
            var menuResultSelectWarpMenu = menuResultWarp?[1];
        Sanderling.MouseClickLeft(menuResultSelectWarpMenu);
            Sanderling.WaitForMeasurement();
            var menuResultats = Measurement?.Menu?.ToList();
		if (Measurement?.Menu?.ToList() ? [1].Entry.ToArray()[x].Text !=  WarpToAnomalyDistance)
			{
			    return TakeAnomaly;
			}
			else
			{
			var menuResultWarpDestination = Measurement?.Menu?.ToList() ? [1].Entry.ToArray();
			
			ClickMenuEntryOnMenuRoot(menuResultWarpDestination[x], WarpToAnomalyDistance);
            
			if (probeScannerWindow != null)
			FlashWindowProbes(2);
            return InBeltMineStep;
			}
		}
    }
    if (null == scanResultCombatSite )
     //   Sanderling.KeyboardPressCombined(new[]{ lockTargetKeyCode, spacekey});
                   if (null == scanResultCombatSite )
            {
                Host.Log("               R2D2: No anomalies, waiting ");
				if( takeateroids && IhaveMinerModule)
				{ WarpToRandomAsteroidsFromFolder(FolderAsteroids, "warp");
				 return InBeltMineStep;}
				else
                return MainStep;
            }
    if (null == scanResultCombatSite )
        {
            while ( L>0)
            {
                if (null == scanResultCombatSite )
                {
                Host.Log("               Trust the Force, Luke  " +L+ "  . ");
                L--;
                return MainStep;
                }
            }
            if (null == scanResultCombatSite )
            {
                Host.Log("               R2D2: no more anomalies! ");
                FinishedMining = true;
				WarpingSlow(RetreatBookmark, "dock");
            }
        }
    return MainStep;
}
//

void FlashWindowProbes( int pp)
{
for (int i=0; i<pp; i++)
    { Sanderling.KeyboardPressCombined(new[] { VirtualKeyCode.LMENU, VirtualKeyCode.VK_P });     Host.Delay(350); }

    
}
public bool BackOnMainStep => (!ReadyForManeuver) || null != RetreatReason;

//////////////////////////////
/////////////////////////////
bool ToUseMiningCrystal()
{
foreach (string moduleWithCrystal in ModuleWithCrystal)
{
    var ModuleCrystal =
		Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule?.Where(module => (module?.TooltipLast?.Value?.LabelText?.Any(
		label => label?.Text?.RegexMatchSuccess(moduleWithCrystal,System.Text.RegularExpressions.RegexOptions.IgnoreCase) ?? false )?? false ));
      if  (ModuleCrystal?.Count() >0)
      return true;

  

        }
        return false;






}
Func<object> InBeltMineStep()
{
     if (BackOnMainStep || SpeedWarping)
        {
        Host.Log("                not on site!");
        return MainStep;
        }
	Host.Log("in site belt");
	if (DefenseEnter && !IamInFleet)
	{
		Host.Log("in site belt, enter defense.");
		WarpingSlow(RetreatBookmark, "dock");
	}


	EnsureWindowInventoryOpenOreHold();

	Host.Log("I am In Fleet ? :  "   +IamInFleet );
 	if(!(KeepWithLeader) && IamInFleet && ListFollowShipOverviewEntry.Length>0)
		FollowLeader();
	if(OreHoldFilledForOffload && IamInFleet && DepositOre)
		{
		Console.Beep(1500, 200); 
		DepositInFleetHangar();
		}
	if(OreHoldFilledForOffload && IamInFleet && !DepositOre)
		{
		Console.Beep(1500, 200); 
		//StockIce();
		}
	if(OreHoldFilledForOffload && !IamInFleet)
		{ 	
		Console.Beep(800, 200);
			GoToUnload = true;
		return MainStep;
		}
	// assigning  or mining

	if (UseToAllarm && ProtectectedByfleetMate ())
	Console.Beep(1500, 200);
	

	
		// daca am targeted atunci calculez distanta si activez modul sau ma apropii/stop
		// daca nu am target atunci ma apropii /stop si lockez
	if (SetTargetAsteroid?.Length >0)
	{

	Host.Log("asteroids near  :    "  +SetTargetAsteroid?.Length );
	var moduleMinerInactive = SetModuleMinerInactive?.FirstOrDefault();
	if (null == moduleMinerInactive)
	{
		Host.Delay(777);
		return InBeltMineStep;
	}
	if (IhavegoodDrones && !(0 < DronesInSpaceCount))
		LaunchDronesByLabelName(@LabelNameMiningDrones);

	if(SetTargetAsteroid?.FirstOrDefault()?.DistanceMax > MiningRange &&!(Measurement?.ShipUi?.Indication?.LabelText?.Any(indicationLabel =>
        (indicationLabel?.Text).RegexMatchSuccessIgnoreCase("approaching")) ?? false))
	{
ClickMenuEntryOnMenuRoot(SetTargetAsteroid?.FirstOrDefault(), "approach");
}
else 
		{
	Host.Log("close");
        	if ((SetTargetAsteroid?.FirstOrDefault()?.DistanceMax > MiningRange) &&   ( Measurement?.ShipUi?.Indication?.LabelText?.Any(indicationLabel => (indicationLabel?.Text).RegexMatchSuccessIgnoreCase("approaching")) ?? false))
        		Sanderling.KeyboardPressCombined(new[]{ lockTargetKeyCode, spacekey});
        			var	setTargetAsteroidInRange	=
		SetTargetAsteroid?.Where(target => target?.DistanceMax <= MiningRange)?.ToArray();

	var setTargetAsteroidInRangeNotAssigned =
		setTargetAsteroidInRange?.Where(target => !(0 < target?.Assigned?.Length))?.ToArray();
		if(0 < setTargetAsteroidInRangeNotAssigned?.Length)
		{
		Host.Log("asteroid non assigned");
		Sanderling.MouseClickLeft(SetTargetAsteroid?.FirstOrDefault());
	foreach (var Module in SetModuleMinerInactive.EmptyIfNull())
		{	if (UseMiningCrystal)
			 {
				LoadMiningCrystalForTarget(Module, setTargetAsteroidInRangeNotAssigned?.FirstOrDefault());
				

				
			 }
		ModuleToggle(Module);
		}
		}
		}

	//	 Sanderling.KeyboardPress(VirtualKeyCode.VK_F);
		if (AnyDroneIdle && (Measurement?.Target?.Length > 0))
		{		
			Host.Log("drones idle");
			//MineTarget();
			Sanderling.KeyboardPress(VirtualKeyCode.VK_F);
			Host.Log("mining a new asteroid");
        	}
		return InBeltMineStep;
	}
	else
	{
		//targeting
		Host.Log("targeting");
		var asteroidsInRange = ListAsteroidOverviewEntry?.Where( range => range?.DistanceMin <= MiningRange)?.OrderByDescending((entry => entry?.DistanceMax?? int.MaxValue))?.ToArray();
	var asteroidOverviewEntryNext = asteroidsInRange?.FirstOrDefault();
	if (0 < ListAsteroidOverviewEntry?.Length)
	{string ListAsteroidsToPrint = String.Join(" ;  ", ListAsteroidOverviewEntry.Select(entry =>entry?.Name).Zip(ListAsteroidOverviewEntry.Select(entry =>entry?.DistanceMax), (l1, l2) => l1+ " at "+ l2));
		Host.Log("ListAsteroidsToPrint:    "+ListAsteroidsToPrint);}

	if(!(0 < ListAsteroidOverviewEntry?.Length) ||!(0 < ListAllAsteroidOverviewEntry?.Length) )
	{
		Host.Log("no asteroids here");
		
		return MainStep;
	}

	if(!(asteroidOverviewEntryNext?.MeTargeting ?? false) || !(asteroidOverviewEntryNext?.MeTargeted ?? false))
	{
		Host.Log("no target");
	if ( !(ListAsteroidOverviewEntry?.FirstOrDefault()?.DistanceMax < MiningRange) 
		|| !(Measurement?.ShipUi?.Indication?.LabelText?.Any(indicationLabel =>
        (indicationLabel?.Text).RegexMatchSuccessIgnoreCase("approaching")) ?? false) )
	{
		Host.Log("closing to asteroid");
		ClickMenuEntryOnMenuRoot(ListAsteroidOverviewEntry?.FirstOrDefault(), "approach");
	}
	else
	{
	        	if ( ListAsteroidOverviewEntry?.FirstOrDefault()?.DistanceMax < MiningRange)
        		Sanderling.KeyboardPressCombined(new[]{ lockTargetKeyCode, spacekey});
		Host.Log("initiate lock asteroid");
		if(asteroidOverviewEntryNext != null)
		ClickMenuEntryOnMenuRoot(asteroidOverviewEntryNext, "^lock");
	}
	}


	}
	return InBeltMineStep;
}
/////////////////////
///////////////////
public bool Tethering =>
    Measurement?.ShipUi?.EWarElement?.Any(EwarElement => (EwarElement?.EWarType).RegexMatchSuccess("tethering")) ?? false;
DroneViewEntryItem[] AllDrones => WindowDrones?.ListView?.Entry?.OfType<DroneViewEntryItem>()?.ToArray();
bool IhavegoodDrones => WindowDrones != null ? ( AllDrones?.Any( name =>name?.LabelText?.FirstOrDefault()?.Text?.RegexMatchSuccessIgnoreCase(LabelNameMiningDrones)??false)??false) : false;
public bool AnyDroneIdle =>AllDrones?.Any(drone =>drone?.LabelText?.FirstOrDefault()?.Text?.Contains("Idle") ?? false ) ?? false;
bool IamInFleet =>listOverviewFleetFriends?.Length > 0;
Sanderling.Parse.IMemoryMeasurement	Measurement	=>
	Sanderling?.MemoryMeasurementParsed?.Value;

IWindow ModalUIElement =>
	Measurement?.EnumerateReferencedUIElementTransitive()?.OfType<IWindow>()?.Where(window => window?.isModal ?? false)
	?.OrderByDescending(window => window?.InTreeIndex ?? int.MinValue)
	?.FirstOrDefault();	

IEnumerable<Parse.IMenu> Menu => Measurement?.Menu;

Parse.IShipUi ShipUi => Measurement?.ShipUi;

IWindowDroneView WindowDrones =>
    Measurement?.WindowDroneView?.FirstOrDefault();
Tab OverviewTabActive =>
	Measurement?.WindowOverview?.FirstOrDefault()?.PresetTab
	?.OrderByDescending(tab => tab?.LabelColorOpacityMilli ?? 1500)
	?.FirstOrDefault();
Tab miningTab => WindowOverview?.PresetTab
	?.OrderByDescending(tab => tab?.Label.Text.RegexMatchSuccessIgnoreCase(MiningTab))
	?.FirstOrDefault();
Sanderling.Interface.MemoryStruct.IMenuEntry MenuEntryLockTarget =>
	Menu?.FirstOrDefault()?.Entry?.FirstOrDefault(entry => entry.Text.RegexMatchSuccessIgnoreCase("^lock"));

Sanderling.Parse.IWindowOverview	WindowOverview	=>
	Measurement?.WindowOverview?.FirstOrDefault();

Sanderling.Parse.IWindowInventory	WindowInventory	=>
	Measurement?.WindowInventory?.FirstOrDefault();


ITreeViewEntry InventoryActiveShipOreHold =>
	WindowInventory?.ActiveShipEntry?.TreeEntryFromCargoSpaceType(ShipCargoSpaceTypeEnum.OreHold);

IInventoryCapacityGauge OreHoldCapacityMilli =>
	(InventoryActiveShipOreHold?.IsSelected ?? false) ? WindowInventory?.SelectedRightInventoryCapacityMilli : null;

int? OreHoldFillPercent => (int?)((OreHoldCapacityMilli?.Used * 100) / OreHoldCapacityMilli?.Max);

Tab OverviewPresetTabActive =>
	WindowOverview?.PresetTab
	?.OrderByDescending(tab => tab?.LabelColorOpacityMilli ?? 0)
	?.FirstOrDefault();

string OverviewTypeSelectionName =>
	WindowOverview?.Caption?.RegexMatchIfSuccess(@"\(([^\)]*)\)")?.Groups?[1]?.Value;

Parse.IOverviewEntry[] ListRatOverviewEntry => WindowOverview?.ListView?.Entry?.Where(entry =>
		(entry?.MainIconIsRed ?? false)	&& (entry?.IsAttackingMe ?? false))
		?.OrderBy(entry => entry?.DistanceMax ?? int.MaxValue)
		?.ToArray();
bool IhaveMinerModule => Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule?.Any(module => !(module?.TooltipLast?.Value?.LabelText?.Any(
		label => label?.Text?.RegexMatchSuccess("ice",System.Text.RegularExpressions.RegexOptions.IgnoreCase) ?? false )?? false ) && (module?.TooltipLast?.Value?.IsMiner??false))?? false ;
bool IhaveICeModule => Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule?.Any(module =>(module?.TooltipLast?.Value?.LabelText?.Any(
		label => label?.Text?.RegexMatchSuccess("ice",System.Text.RegularExpressions.RegexOptions.IgnoreCase) ?? false )?? false ) || (module?.TooltipLast?.Value?.IsIceHarvester??false))?? false ;
DroneViewEntryGroup DronesInBayListEntry =>
	WindowDrones?.ListView?.Entry?.OfType<DroneViewEntryGroup>()?.FirstOrDefault(Entry => null != Entry?.Caption?.Text?.RegexMatchIfSuccess(@"Drones in bay", RegexOptions.IgnoreCase));

DroneViewEntryGroup DronesInSpaceListEntry =>
	WindowDrones?.ListView?.Entry?.OfType<DroneViewEntryGroup>()?.FirstOrDefault(Entry => null != Entry?.Caption?.Text?.RegexMatchIfSuccess(@"Drones in Local Space", RegexOptions.IgnoreCase));

int?	DronesInSpaceCount => DronesInSpaceListEntry?.Caption?.Text?.AsDroneLabel()?.Status?.TryParseInt();

bool ReadyForManeuverNot =>
	Measurement?.ShipUi?.Indication?.LabelText?.Any(indicationLabel =>
		(indicationLabel?.Text).RegexMatchSuccessIgnoreCase("warp|docking")) ?? false;

bool ReadyForManeuver => !ReadyForManeuverNot && !(Measurement?.IsDocked ?? true);

Sanderling.Parse.IShipUiTarget[] SetTargetAsteroid =>
	Measurement?.Target?.Where(target =>
		target?.TextRow?.Any(textRow => textRow.RegexMatchSuccessIgnoreCase("asteroid")) ?? false)?.ToArray();

Sanderling.Interface.MemoryStruct.IListEntry	WindowInventoryItem	=>
	WindowInventory?.SelectedRightInventory?.ListView?.Entry?.FirstOrDefault();

Sanderling.Accumulation.IShipUiModule[] SetModuleMiner =>
	Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule?.Where(module => (module?.TooltipLast?.Value?.IsIceHarvester ?? false) || (module?.TooltipLast?.Value?.IsMiner?? false) )?.ToArray();

Sanderling.Accumulation.IShipUiModule[] SetModuleMinerInactive	 =>
	SetModuleMiner?.Where(module => !(module?.RampActive ?? false))?.ToArray();

int?	MiningRange => SetModuleMiner?.Select(module =>
	module?.TooltipLast?.Value?.RangeOptimal ?? module?.TooltipLast?.Value?.RangeMax ?? module?.TooltipLast?.Value?.RangeWithin ?? 0)?.DefaultIfEmpty(0)?.Min();;

WindowChatChannel chatLocal =>
	 Sanderling.MemoryMeasurementParsed?.Value?.WindowChatChannel
	 ?.FirstOrDefault(windowChat => windowChat?.Caption?.RegexMatchSuccessIgnoreCase("local") ?? false);
var logoutme= false;
var logoutgame = (eveRealServerDT-DateTime.UtcNow ).TotalMinutes;
//    assuming that own character is always visible in local
bool hostileOrNeutralsInLocal => IgnoreListCount < chatLocal?.ParticipantView?.Entry?.Count(IsNeutralOrEnemy);

//	extract the ore type from the name as seen in overview. "Asteroid (Plagioclase)"
string OreTypeFromAsteroidName(string AsteroidName)	=>
	AsteroidName.ValueFromRegexMatchGroupAtIndex(@"Asteroid \(([^\)]+)", 0);

void LoadMiningCrystalForTarget(Sanderling.Accumulation.IShipUiModule module, Sanderling.Parse.IShipUiTarget target)
{
	string[] targetTextRow = target?.TextRow;
	if (targetTextRow != null && targetTextRow.Length > 0) {
		string baseOre = BaseOreFromAsteroidName(String.Join(" ", targetTextRow));
		if (!String.IsNullOrEmpty(baseOre)) {
			Sanderling.MouseMove(module);
			Sanderling.WaitForMeasurement();
			var foundCrystal = module?.TooltipLast?.Value?.LabelText?.FirstOrDefault(entry => entry.Text.RegexMatchSuccessIgnoreCase("Mining Crystal"));
			//IEnumerable<Sanderling.Interface.MemoryStruct.UIElementText> foundLabels = module?.TooltipLast?.Value?.LabelText?.Text.Where(lbl => lbl.Text.Contains("Mining Crystal"));
			
			if (foundCrystal != null && foundCrystal.Text.Contains(baseOre))
				return;
			
			Sanderling.MouseClickRight(module);
			var menuCrystal = Menu?.FirstOrDefault()?.EntryFirstMatchingRegexPattern(baseOre +".*mining.*crystal", RegexOptions.IgnoreCase);
			
			if (menuCrystal != null)
				Sanderling.MouseClickLeft(menuCrystal);
			else if (foundCrystal != null)
				Sanderling.MouseClickLeft( Menu?.FirstOrDefault()?.EntryFirstMatchingRegexPattern("unload to cargo", RegexOptions.IgnoreCase));
			else
				Sanderling.MouseClickLeft(target);
		}
	}
}
void ClickMenuEntryOnMenuRoot(IUIElement MenuRoot, string MenuEntryRegexPattern)
{
    Sanderling.MouseClickRight(MenuRoot);
    var Menu = Measurement?.Menu?.FirstOrDefault();
    var MenuEntry = Menu?.EntryFirstMatchingRegexPattern(MenuEntryRegexPattern, RegexOptions.IgnoreCase);
    Sanderling.MouseClickLeft(MenuEntry);
}
void EnsureWindowInventoryOpen()
{
	if (null != WindowInventory)
		return;

	Host.Log("open Inventory.");
	Sanderling.MouseClickLeft(Measurement?.Neocom?.InventoryButton);
}

void EnsureWindowInventoryOpenOreHold()
{
	EnsureWindowInventoryOpen();

	var inventoryActiveShip = WindowInventory?.ActiveShipEntry;

	if(InventoryActiveShipOreHold == null && !(inventoryActiveShip?.IsExpanded ?? false))
		Sanderling.MouseClickLeft(inventoryActiveShip?.ExpandToggleButton);

	if(!(InventoryActiveShipOreHold?.IsSelected ?? false))
		Sanderling.MouseClickLeft(InventoryActiveShipOreHold);
}

//	sample label text: Intensive Reprocessing Array <color=#66FFFFFF>1,123 m</color>
string InventoryContainerLabelRegexPatternFromContainerName(string containerName) =>
	@"^\s*" + Regex.Escape(containerName) + @"\s*($|\<)";

void InInventoryUnloadItems() => InInventoryUnloadItemsTo(UnloadDestContainerName);

void InInventoryUnloadItemsTo(string DestinationContainerName)
{
	Host.Log("unload items to '" + DestinationContainerName + "'.");

		var DestinationContainerLabelRegexPattern =
			InventoryContainerLabelRegexPatternFromContainerName(DestinationContainerName);

		var DestinationContainer =
			WindowInventory?.LeftTreeListEntry?.SelectMany(entry => new[] { entry }.Concat(entry.EnumerateChildNodeTransitive()))
			?.FirstOrDefault(entry => entry?.Text?.RegexMatchSuccessIgnoreCase(DestinationContainerLabelRegexPattern) ?? false);
	EnsureWindowInventoryOpenOreHold();

	for (;;)
	{
		var oreHoldListItem = WindowInventory?.SelectedRightInventory?.ListView?.Entry?.ToArray();

		var oreHoldItem = oreHoldListItem?.FirstOrDefault();

		if(null == oreHoldItem)
			break;    //    0 items in OreHold

		if(1 < oreHoldListItem?.Length)
			ClickMenuEntryOnMenuRoot(oreHoldItem, @"select\s*all");


		if (null == DestinationContainer)
			Host.Log("error: Inventory entry labeled '" + DestinationContainerName + "' not found");

		Sanderling.MouseDragAndDrop(oreHoldItem, DestinationContainer);
	}
	        Host.Delay(1111);
        Sanderling.MouseClickLeft(DestinationContainer);
        ClickMenuEntryOnMenuRoot(WindowInventory?.SelectedRightInventory?.ListView?.Entry?.FirstOrDefault(), @"stack all");
        Host.Log("               Stack All in '" + UnloadDestContainerName+ "' .");
}


void ClickMenuEntryOnPatternMenuRoot(IUIElement MenuRoot, string MenuEntryRegexPattern, string SubMenuEntryRegexPattern = null)
{
    Sanderling.MouseClickRight(MenuRoot);
    var Menu = Sanderling?.MemoryMeasurementParsed?.Value?.Menu?.FirstOrDefault();
    var MenuEntry = Menu?.EntryFirstMatchingRegexPattern(MenuEntryRegexPattern, RegexOptions.IgnoreCase);
    Sanderling.MouseClickLeft(MenuEntry);
    if (SubMenuEntryRegexPattern != null)
    {
		var subMenu = Sanderling?.MemoryMeasurementParsed?.Value?.Menu?.ElementAtOrDefault(1);
        var subMenuEntry = subMenu?.EntryFirstMatchingRegexPattern(SubMenuEntryRegexPattern, RegexOptions.IgnoreCase);
        Sanderling.MouseClickLeft(subMenuEntry);
    }
}
int K=1;
void WarpingSlow(string destination, string action)
{
	Console.Beep(1500, 200);
    K=1;
    ClickMenuEntryOnPatternMenuRoot(Sanderling?.MemoryMeasurementParsed?.Value?.InfoPanelCurrentSystem?.ListSurroundingsButton, destination, action);
}

void Undock()
{
	GoToUnload = false;
	while(Measurement?.IsDocked ?? true)
	{
		Sanderling.MouseClickLeft(Measurement?.WindowStation?.FirstOrDefault()?.ButtonText?.FirstOrDefault(undock =>undock?.Text?.RegexMatchSuccessIgnoreCase("undock")??false  ));
                    
		Host.Log("waiting for undocking to complete.");
		Host.Delay(8000);
	}

	Host.Delay(4444);
	if (Tethering)
		Sanderling.KeyboardPressCombined(new[]{ lockTargetKeyCode, spacekey});
	Sanderling.InvalidateMeasurement();
}

void ModuleMeasureAllTooltip()
{
    var moduleUnknownCount = Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule?.Count((module => null == module?.TooltipLast?.Value?.LabelText?.Any()));
    var initialmoduleCount = moduleUnknownCount;


    while( moduleUnknownCount >0	)
	{
		if ( Measurement?.IsDocked ?? false)
			break;
        for (int i = 0; i < Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule?.Count(); ++i)
		{
            var NextModule = Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule?.ElementAtOrDefault(i);
			if(!ReadyForManeuver)
				break;
			if(null == NextModule)
				break;
			Sanderling.MouseMove(NextModule);
            Host.Delay(305);
			Sanderling.WaitForMeasurement();

                Host.Delay(305);
			Sanderling.MouseMove(NextModule);

            Host.Log("               Detected a new module: " +Measurement?.ModuleButtonTooltip?.LabelText?.ElementAtOrDefault(1)?.Text?.RemoveXmlTag() + "");


		}


        moduleUnknownCount = Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule?.Count((module => null == module?.TooltipLast?.Value?.LabelText?.Any()));
        Host.Log("               Updated counted modules from " + initialmoduleCount+ " to : " +moduleUnknownCount+"");
        if( ToUseMiningCrystal())
        UseMiningCrystal = true;

    }
}

void ActivateHardenerExecute()
{
	var	SubsetModuleHardener =
		Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule
		?.Where(module => module?.TooltipLast?.Value?.IsHardener ?? false);

	var	SubsetModuleToToggle =
		SubsetModuleHardener
		?.Where(module => !(module?.RampActive ?? false));

	foreach (var Module in SubsetModuleToToggle.EmptyIfNull())
		ModuleToggle(Module);
}

void ModuleToggle(Sanderling.Accumulation.IShipUiModule Module)
{
	var ToggleKey = Module?.TooltipLast?.Value?.ToggleKey;

	Host.Log("toggle module using " + (null == ToggleKey ? "mouse" : Module?.TooltipLast?.Value?.ToggleKeyTextLabel?.Text));

	if(null == ToggleKey)
		Sanderling.MouseClickLeft(Module);
	else
		Sanderling.KeyboardPressCombined(ToggleKey);
}

void MemoryUpdate()
{
	
	RetreatUpdate();
	
	OffloadCountUpdate();
	Timers ();

}
void Timers ()
{

    var now = DateTime.UtcNow;
    var CloseGameSession = (playSession - now).TotalMinutes;
    var CloseGameDT = (eveSafeDT - now).TotalMinutes;
    var LogoutGame = Math.Min(CloseGameDT,CloseGameSession);
    if (playSession !=DateTime.UtcNow)
        logoutgame = LogoutGame;
	if (LogoutGame < 0) 
	{
	    logoutme = true;
		Host.Log("               Logoutgame is" + logoutme + " ");
	}
}


bool MeasurementEmergencyWarpOutEnter =>
	!(Measurement?.IsDocked ?? false) && !(EmergencyWarpOutHitpointPercent < ShieldHpPercent);

void RetreatUpdate()
{
     if (RetreatOnNeutralOrHostileInLocal && hostileOrNeutralsInLocal && !(Measurement?.IsDocked ?? false))
 Console.Beep(1500, 200);

	RetreatReasonTemporary = ((RetreatOnNeutralOrHostileInLocal && !IamInFleet && hostileOrNeutralsInLocal) || logoutme ||(listOverviewEntryEnemy?.Length > 0 &&RetreatOnNeutralOrHostileInLocal && IamInFleet && hostileOrNeutralsInLocal) )	? "hostile or neutral in local" : null;

	if (!MeasurementEmergencyWarpOutEnter)
		return;

	//	measure multiple times to avoid being scared off by noise from a single measurement. 
	Sanderling.InvalidateMeasurement();

	if (!MeasurementEmergencyWarpOutEnter)
		return;

	RetreatReasonPermanent = "shield hp";
}

void OffloadCountUpdate()
{
	var	OreHoldFillPercentSynced	= OreHoldFillPercent;

	if(!OreHoldFillPercentSynced.HasValue)
		return;

	if(0 == OreHoldFillPercentSynced && OreHoldFillPercentSynced < LastCheckOreHoldFillPercent)
		++OffloadCount;

	LastCheckOreHoldFillPercent = OreHoldFillPercentSynced;
}
int ? IgnoreListCount  => chatLocal?.ParticipantView?.Entry?.Where(ignoreplayer =>ignoreplayer?.NameLabel?.Text?.RegexMatchSuccessIgnoreCase(IgnoreNeutral) ?? false).ToArray()?.Count() ;

void FollowLeader()
{

            
    Sanderling.KeyDown(VirtualKeyCode.VK_E);
         Sanderling.MouseClickLeft(ListFollowShipOverviewEntry?.FirstOrDefault());
    Sanderling.KeyUp(VirtualKeyCode.VK_E);
}




void DepositInFleetHangar()
{

        Host.Delay(1111);
			var FleetContainerLabelRegexPattern =
			InventoryContainerLabelRegexPatternFromContainerName(UnloadDestFleetContainer);

		var FleetContainer =
			WindowInventory?.LeftTreeListEntry?.SelectMany(entry => new[] { entry }.Concat(entry.EnumerateChildNodeTransitive()))
			?.FirstOrDefault(entry => entry?.Text?.RegexMatchSuccessIgnoreCase(FleetContainerLabelRegexPattern) ?? false);
Host.Log(FleetContainer);
		if (null == FleetContainer)
		ClickMenuEntryOnMenuRoot(ListFollowShipOverviewEntry?.FirstOrDefault(), "open fleet hangar");
		EnsureWindowInventoryOpenOreHold();
    var RefillListItem = WindowInventory?.SelectedRightInventory?.ListView?.Entry
    ?.Where(entry => !(entry?.LabelText?.FirstOrDefault()?.Text?.RegexMatchSuccessIgnoreCase("compressed") ?? false))
    ?.ToArray();
    if (!RefillListItem?.IsNullOrEmpty() ?? false)
   Host.Log("              Filling with : " +RefillListItem?.FirstOrDefault().LabelText.FirstOrDefault().Text);
    var RefillItem = RefillListItem?.FirstOrDefault();
    
        Host.Delay(511);
    Sanderling.MouseDragAndDrop(RefillItem , FleetContainer);

    Host.Delay(811);
   // Sanderling.MouseClickLeft(WindowInventory?.ActiveShipEntry);
}


Parse.IOverviewEntry[] ListAllAsteroidOverviewEntry =>
	WindowOverview?.ListView?.Entry
	?.Where(entry => null != OreTypeFromAsteroidName(entry?.Name) )
	?.ToArray();
	

bool KeepWithLeader =>
    Measurement?.ShipUi?.Indication?.LabelText?.Any(indicationLabel =>
        (indicationLabel?.Text).RegexMatchSuccessIgnoreCase("keeping")) ?? false;
Parse.IOverviewEntry[] listOverviewEntryFriends =>
    WindowOverview?.ListView?.Entry
    ?.Where(entry => entry?.ListBackgroundColor?.Any(IsFriendBackgroundColor) ?? false)
    ?.ToArray();
Parse.IOverviewEntry[] listOverviewFleetFriends =>
    WindowOverview?.ListView?.Entry
    ?.Where(entry => entry?.ListBackgroundColor?.Any(IsFleetBackgroundColor) ?? false)
    ?.ToArray();
Parse.IOverviewEntry[] listOverviewEntryEnemy =>
    WindowOverview?.ListView?.Entry
    ?.Where(entry => entry?.ListBackgroundColor?.Any(IsEnemyBackgroundColor) ?? false)
    ?.ToArray();
Parse.IOverviewEntry[] ListFollowShipOverviewEntry => WindowOverview?.ListView?.Entry
	?.Where(entry => 
	(entry?.Type?.RegexMatchSuccessIgnoreCase(FollowShipType, RegexOptions.IgnoreCase ) ?? false)
	&& (entry?.Name?.RegexMatchSuccessIgnoreCase(FollowChiefName, RegexOptions.IgnoreCase ) ?? false)
	&& (entry?.ListBackgroundColor?.Any(IsFleetBackgroundColor) ?? false)
	
	)
    ?.OrderBy(entry => entry?.DistanceMax ?? int.MaxValue)
    ?.ToArray();

string BaseOreFromAsteroidName(string asteroidName)
{
    return Array.Find<string>(BaseOre, (Predicate<string>)delegate (string s)
    {
        return asteroidName.IndexOf(s, StringComparison.OrdinalIgnoreCase) > -1;
    });
}
int? MiningPriorityFromOverviewEntry(Parse.IOverviewEntry overviewEntry) =>
	ModifierOrePreference?.FirstIndexOrNull(oreClass =>overviewEntry?.Name?.RegexMatchSuccessIgnoreCase(oreClass) ?? false );

IGrouping<int?, Parse.IOverviewEntry>[] OverviewAsteroidEntriesGroupedByPriority =>
	WindowOverview?.ListView?.Entry
	?.GroupBy(MiningPriorityFromOverviewEntry)
	?.Where(group => group.Key != null)
	?.OrderBy(group => group.Key)
	?.ToArray();
Parse.IOverviewEntry[] ListAsteroidOverviewEntry =>
	OverviewAsteroidEntriesGroupedByPriority
	?.FirstOrDefault()?.ToArray();
bool AskedForProtection;
string FleetCharFlagList => chatLocal?.ParticipantView?.Entry?.Where(fleetmember =>(fleetmember?.FlagIcon?.FirstOrDefault()?.HintText?.RegexMatchSuccessIgnoreCase("Pilot is in your fleet") ?? false) &&(fleetmember?.NameLabel?.Text?.RegexMatchSuccessIgnoreCase(FleetMate) ?? false)).ToArrayIfNotEmpty()?.FirstOrDefault()?.NameLabel.Text  ;
int RandomInt() => new Random((int)Host.GetTimeContinuousMilli()).Next();
T RandomElement<T>(IEnumerable<T> sequence)
{
    var array = (sequence as T[]) ?? sequence?.ToArray();
 
    if (!(0 < array?.Length))
        return default(T);
 
    return array[RandomInt() % array.Length];
}
void WarpToRandomAsteroidsFromFolder(string FolderAsteroids,  string actions)
{
	Host.Log("               Take one asteroid in place of anomaly");
var listSurroundingsButton = Measurement?.InfoPanelCurrentSystem?.ListSurroundingsButton;

Sanderling.MouseClickRight(listSurroundingsButton);
var bookmarkMenuEntry = Measurement?.Menu?.FirstOrDefault()?.EntryFirstMatchingRegexPattern("^" + FolderAsteroids + "$", RegexOptions.IgnoreCase);
Sanderling.MouseClickRight(bookmarkMenuEntry);
var Menu = Sanderling?.MemoryMeasurementParsed?.Value?.Menu?.ElementAtOrDefault(1);
var SecondLevelMenuEntry = RandomElement(Menu.Entry?.ToArray());
Sanderling.MouseClickLeft(SecondLevelMenuEntry);
if (actions != null)
{
var subMenu = Sanderling?.MemoryMeasurementParsed?.Value?.Menu?.ElementAtOrDefault(2);
var subMenuEntry = subMenu?.EntryFirstMatchingRegexPattern(actions, RegexOptions.IgnoreCase);
Sanderling.MouseClickLeft(subMenuEntry);
}
}
bool ProtectectedByfleetMate ()
	{
//	var WindowFleet =Sanderling?.MemoryMeasurement?.Value?.WindowStack?.Where(myfleet =>myfleet?.LabelText?.ElementAtOrDefault(1)?.Text?.RegexMatchSuccessIgnoreCase("my fleet") ??false);
	var WindowFleet =Sanderling?.MemoryMeasurement?.Value?.WindowOther?.Where(myfleet =>myfleet?.LabelText?.ElementAtOrDefault(1)?.Text?.RegexMatchSuccessIgnoreCase("my fleet") ??false);

		if ((Measurement?.IsDocked ?? false) || AskedForProtection || FleetCharFlagList == null || WindowFleet== null || !(0 < ListRatOverviewEntry?.Length) )
		return false;


if (!(WindowFleet?.FirstOrDefault()?.ButtonText?.FirstOrDefault()?.Text?.RegexMatchSuccessIgnoreCase("Clear History")?? false) )
Sanderling.MouseClickLeft(WindowFleet?.FirstOrDefault()?.LabelText?.Where(text =>text?.Text?.RegexMatchSuccessIgnoreCase("History") ?? false).FirstOrDefault());
				
if ((WindowFleet?.FirstOrDefault()?.ButtonText?.FirstOrDefault()?.Text?.RegexMatchSuccessIgnoreCase("Clear History")?? false)&& !AskedForProtection  && 0 < ListRatOverviewEntry?.Length  )
{
	
	AskedForProtection =true;
	Sanderling.MouseClickLeft(WindowFleet?.FirstOrDefault()?.LabelText?.ElementAtOrDefault(6));
	Host.Delay(200);
	Sanderling.KeyboardPress(VirtualKeyCode.VK_Z);
	Host.Log("               Allarm for Asking protection");
	Host.Delay(200);
	Sanderling.MouseClickLeft(WindowFleet?.FirstOrDefault()?.ButtonText?.Where(text =>text?.Text?.RegexMatchSuccessIgnoreCase("Clear History") ?? false).FirstOrDefault());
return true;
}
return false;
}
void Expander(string Label)
{
	var Element = Measurement?.WindowDroneView?.FirstOrDefault()?.ListView?.Entry?.FirstOrDefault(w => w?.LabelText?.FirstOrDefault()?.Text?.RegexMatchSuccessIgnoreCase("^" + Label) ?? false);
		bool IsExpanded = Element?.IsExpanded ?? true;
    if(!IsExpanded) ClickMenuEntryOnMenuRoot(Element,"Expand");
    var SpaceElement = Measurement?.WindowDroneView?.FirstOrDefault()?.ListView?.Entry?.LastOrDefault(w => w?.LabelText?.FirstOrDefault()?.Text?.RegexMatchSuccessIgnoreCase("^" + Label) ?? false);
	bool IsSpaceExpanded = SpaceElement?.IsExpanded ?? true;
	if(!IsSpaceExpanded) ClickMenuEntryOnMenuRoot(SpaceElement,"Expand");
}
void LaunchDronesByLabelName(string LabelName)
{
	Expander("Drones in Bay");
	var Label = Measurement?.WindowDroneView?.FirstOrDefault()?.LabelText?.FirstOrDefault(w => w.Text.RegexMatchSuccessIgnoreCase("^" + LabelName) );
	if(ReadyForManeuver)
	ClickMenuEntryOnMenuRoot(Label, "^Launch Drones");
}
bool AnomalySuitableGeneral(MemoryStruct.IListEntry scanResult) =>
    scanResult?.CellValueFromColumnHeader(AnomalyToTakeColumnHeader)?.RegexMatchSuccessIgnoreCase(AnomalyToTake) ?? false;
bool ActuallyAnomaly(MemoryStruct.IListEntry scanResult) =>
       scanResult?.CellValueFromColumnHeader("Distance")?.RegexMatchSuccessIgnoreCase("km") ?? false;
bool IgnoreAnomaly(MemoryStruct.IListEntry scanResult) =>
!(scanResult?.CellValueFromColumnHeader(AnomalyToTakeColumnHeader)?.RegexMatchSuccessIgnoreCase(AnomalyToTake) ?? false);
bool IsEnemyBackgroundColor(ColorORGB color) =>
    color.OMilli == 500 && color.RMilli == 750 && color.GMilli == 0 && color.BMilli == 0;
bool IsFriendBackgroundColor(ColorORGB color) =>
    (color.OMilli == 500 && color.RMilli == 600 && color.GMilli == 150 && color.BMilli == 900) || (color.OMilli == 500 && color.RMilli == 0 && color.GMilli == 150 && color.BMilli == 600) || (color.OMilli == 500 && color.RMilli == 100 && color.GMilli == 600 && color.BMilli == 100);
bool IsNeutralOrEnemy(IChatParticipantEntry participantEntry) =>(participantEntry?.FlagIcon != null) &&
   !(participantEntry?.FlagIcon?.Any(flagIcon =>
     new[] { "good standing", "excellent standing", "Pilot is in your (fleet|corporation|alliance)", "Pilot is an ally in one or more of your wars", }
     .Any(goodStandingText =>
        flagIcon?.HintText?.RegexMatchSuccessIgnoreCase(goodStandingText) ?? false)) ?? false);
bool IsFleetBackgroundColor(ColorORGB color) =>
    (color.OMilli == 500 && color.RMilli == 600 && color.GMilli == 150 && color.BMilli == 900);
public bool SpeedWarping => Measurement?.ShipUi?.SpeedLabel?.Text?.RegexMatchSuccessIgnoreCase("(Warping)") ?? false;
///////////
///to add on marvel script
/*
bool ProtectingFleetMate ()
{
	//var WindowFleet =Sanderling?.MemoryMeasurement?.Value?.WindowStack?.Where(myfleet =>myfleet?.LabelText?.ElementAtOrDefault(1)?.Text?.RegexMatchSuccessIgnoreCase("my fleet") ??false);
var WindowFleet = Sanderling?.MemoryMeasurement?.Value?.WindowOther?.Where(myfleet =>myfleet?.LabelText?.ElementAtOrDefault(1)?.Text?.RegexMatchSuccessIgnoreCase("my fleet") ??false);

		if (  (FleetCharFlagList?.IsNullOrEmpty() ?? false) || WindowFleet== null)
		return false;


if (!(WindowFleet?.FirstOrDefault()?.ButtonText?.FirstOrDefault()?.Text?.RegexMatchSuccessIgnoreCase("Clear History")?? false) )
Sanderling.MouseClickLeft(WindowFleet?.FirstOrDefault()?.LabelText?.Where(text =>text?.Text?.RegexMatchSuccessIgnoreCase("History") ?? false).FirstOrDefault());

if ((WindowFleet?.FirstOrDefault()?.ButtonText?.FirstOrDefault()?.Text?.RegexMatchSuccessIgnoreCase("Clear History")?? false)  )
{
	var EventTrigger = WindowFleet?.FirstOrDefault()?.LabelText?.ElementAtOrDefault(6)?.Text?.RegexMatchSuccessIgnoreCase("spotted an enemy")?? false;
	if(EventTrigger)
{
    Host.Log("               Called by dutty");


	//Sanderling.MouseClickLeft(WindowFleet?.FirstOrDefault()?.ButtonText?.Where(text =>text?.Text?.RegexMatchSuccessIgnoreCase("Clear History") ?? false).FirstOrDefault());
return true;
}
}
return false;
}
//and after
//if(Measurement?.WindowTelecom != null)
//    CloseWindowTelecom();

if ( ProtectingFleetMate ())
{
    //var WindowFleet = Sanderling?.MemoryMeasurement?.Value?.WindowStack?.Where(myfleet =>myfleet?.LabelText?.ElementAtOrDefault(1)?.Text?.RegexMatchSuccessIgnoreCase("my fleet") ??false);
    var WindowFleet =Sanderling?.MemoryMeasurement?.Value?.WindowOther?.Where(myfleet =>myfleet?.LabelText?.ElementAtOrDefault(1)?.Text?.RegexMatchSuccessIgnoreCase("my fleet") ??false);

    ModuleStopToggle(ModuleAfterburner);
    ActivateArmorExecute();
    Host.Log("               have to go protect my fleet mate");
    if (ActivateShieldBooster == true)
    {

    ModuleToggle(ModuleShieldBooster);
    }
    while (DronesInSpaceCount>0 )
    DroneEnsureInBay();
    ClickMenuEntryOnMenuRoot(WindowFleet?.FirstOrDefault()?.LabelText?.ElementAtOrDefault(6) , "warp");

}
*/
// inside  while (SpeedWarping)
/* 
    var WindowFleet =Sanderling?.MemoryMeasurement?.Value?.WindowOther?.Where(myfleet =>myfleet?.LabelText?.ElementAtOrDefault(1)?.Text?.RegexMatchSuccessIgnoreCase("my fleet") ??false);

    var EventTrigger = WindowFleet?.FirstOrDefault()?.LabelText?.ElementAtOrDefault(6)?.Text?.RegexMatchSuccessIgnoreCase("spotted an enemy")?? false;
	if(EventTrigger)
    Sanderling.MouseClickLeft(WindowFleet?.FirstOrDefault()?.ButtonText?.Where(text =>text?.Text?.RegexMatchSuccessIgnoreCase("Clear History") ?? false).FirstOrDefault());
*/
