new AudioProfile(MinesHitMine) {
	fileName = "./sounds/boom.wav";
	description = AudioGui;
	preload = true;
};
new AudioProfile(MinesWinSound : MinesHitMine) { fileName = "./sounds/win.wav"; };
new AudioProfile(MinesStartSound : MinesHitMine) { fileName = "./sounds/start.wav"; };

//http://forum.blockland.us/index.php?topic=204513.msg5651092#msg5651092
$remapDivision[$remapCount] = "Minesweeper";
$remapName[$remapCount] = "Launch Minesweeper";
$remapCmd[$remapCount] = "Mines_pushDialogKB";
$remapCount++;

function Mines_pushDialogKB(%x) {
	if(%x) { 
		Mines_pushDialog();
	}
}
//end thing from link

if(isObject(MinesGUI)) {
	Mines_endGame(1);
	MinesGUI.delete();
}
exec("./MinesGUI.gui");
if(isObject(MinesSettings)) {
	MinesSettings.delete();
}
exec("./MinesSettings.gui");

if(!isFile("config/client/mines/prefs.cs")) {
	$Mines::Settings::GridWidth = 16;
	$Mines::Settings::GridHeight = 16;
	$Mines::Settings::Mines = 30;
	$Mines::Settings::GridSize = 20;
	export("$Mines::Settings::*","config/client/mines/prefs.cs",false);
}
exec("config/client/mines/prefs.cs");

$Mines::GridWidth = $Mines::Settings::GridWidth;
$Mines::GridHeight = $Mines::Settings::GridHeight;
$Mines::Mines = $Mines::Settings::Mines;
$Mines::GridSize = 20;
$Mines::MinesRemaining = $Mines::Mines;
$Mines::ButtonCount = 0;

function Mines_init() {
	$Mines::ButtonCount = 0;

	if($Mines::GridWidth < 8) {
		$Mines::GridWidth = 8;
	}
	if($Mines::GridHeight < 8) {
		$Mines::GridHeight = 8;
	}
	if($Mines::Mines < 5) {
		$Mines::Mines = 5;
	}

	%grid_width = $Mines::GridWidth;
	%grid_height = $Mines::GridHeight;
	%grid_size = $Mines::GridSize;
	%mine_total = $Mines::GridWidth*$Mines::GridHeight;

	if($Mines::Mines > %mine_total-1) {
		$Mines::Mines = %mine_total-1;
	}

	Mines_Window.extent = 121 + (%grid_size * %grid_width) SPC 41 + (%grid_size * %grid_height);
	Mines_Area.extent = 1 + (%grid_size * %grid_width) SPC 1 + (%grid_size * %grid_height);
	Mines_StatsArea.extent = "100" SPC 1 + (%grid_size * %grid_height);
	Mines_StatsArea.position = getWord(Mines_Area.extent,0) + 10 SPC getWord(Mines_StatsArea.position,1);
	Mines_Window.setText("Mines -" SPC %grid_width @ "x" @ %grid_height @ "," SPC $Mines::Mines SPC "mines");
	Mines_ScoreText.setText("<just:center><color:0000cc><font:Impact:20>" @ $Mines::MinesRemaining);
	
	%ext = $Mines::GridSize-1 SPC $Mines::GridSize-1;
	
	for(%i=0;%i<%grid_width;%i++) {
		for(%j=0;%j<%grid_height;%j++) {
			%pos = 1 + (%i*$Mines::GridSize) SPC 1 + (%j*$Mines::GridSize);

			%grid = new GuiSwatchCtrl("Mine_Grid" SPC %i SPC %j) {
				profile = "GuiDefaultProfile";
				horizSizing = "right";
				vertSizing = "bottom";
				position = %pos;
				extent = %ext;
				minExtent = %ext;
				enabled = "1";
				visible = "1";
				clipToParent = "1";
				color = "255 255 255 64";
				mine = "0";
				gridX = %i;
				gridY = %j;
			};
			Mines_Area.add(%grid);
		}
	}

	$Mines::MinesRemaining = $Mines::Mines;
	$Mines::Time = 0;
}

function Mines_pushDialog() {
	canvas.pushDialog(MinesGUI);
	Mines_init();
}

function Mines_popDialog(%state) {
	%temp = Mines_Window.position;
	canvas.popDialog(MinesGUI);
	MinesGUI.delete();
	exec("Add-Ons/Client_Mines/MinesGUI.gui");
	cancel($MinesTimer);
	if(%state) {
		Mines_pushDialog();
		Mines_Window.position = %temp;
	}
}

function Mines_initButtons() {
	%ext = $Mines::GridSize SPC $Mines::GridSize;

	for(%i=0;%i<$Mines::GridWidth;%i++) {
		for(%j=0;%j<$Mines::GridHeight;%j++) {

			%text = new GuiMLTextCtrl("Mine_Grid_Text" SPC %i SPC %j) {
				profile = "GuiMLTextProfile";
				horizSizing = "left";
				vertSizing = "bottom";
				//position = 0 SPC ($Mines::GridSize-20)/2; 
				position = "0 0";
				//extent = $Mines::GridSize SPC $Mines::GridSize-(($Mines::GridSize-20)/2);
				extent = %ext;
				minExtent = %ext;
				enabled = "1";
				visible = "1";
				clipToParent = "1";
				lineSpacing = "2";
				allowColorChars = "0";
				maxChars = "-1";
				text = "";
				maxBitmapHeight = "-1";
				selectable = "1";
				autoResize = "0";
			};
			("Mine_Grid" SPC %i SPC %j).add(%text);

			%button = new GuiBitmapButtonCtrl("Mine_Grid_Button" SPC %i SPC %j) {
				horizSizing = "right";
				vertSizing = "bottom";
				position = "0 0";
				extent = %ext;
				minExtent = %ext;
				enabled = "1";
				visible = "1";
				clipToParent = "1";
				command = "Mines_clickGrid(" @ %i @ ", " @ %j @ ");";
				altCommand = "Mines_markFlag(" @ %i @ ", " @ %j @ ", 0);";
				text = "";
				groupNum = "-1";
				buttonType = "PushButton";
				bitmap = "Add-Ons/Client_Mines/images/button";
				lockAspectRatio = "0";
				alignLeft = "0";
				alignTop = "0";
				overflowImage = "0";
				mKeepCached = "0";
				mColor = "255 255 255 255";
       		};
			("Mine_Grid" SPC %i SPC %j).add(%button);
		}
	}
	$Mines::ButtonCount = $Mines::GridWidth*$Mines::GridHeight;
}

function Mines_Start() {
	Mines_popDialog(1);
	$Mines::MinesRemaining = $Mines::Mines;
	$Mines::StartTime = getRealTime();
	Mines_initButtons();
	Mines_initMines();
	Mines_initNumbers();
	Mines_initTimer();
	alxPlay(MinesStartSound);
}

function Mines_initMines()
{
	%grid_total = $Mines::GridWidth * $Mines::GridHeight;

	for(%i=0;%i<$Mines::Mines;%i++) {
		%x = getRandom(0,$Mines::GridWidth-1);
		%y = getRandom(0,$Mines::GridHeight-1);

		while(("Mine_Grid" SPC %x SPC %y).mine) {
			%x = getRandom(0,$Mines::GridWidth-1);
			%y = getRandom(0,$Mines::GridHeight-1);
		}

		("Mine_Grid" SPC %x SPC %y).mine = 1;
		("Mine_Grid" SPC %x SPC %y).setColor("255 0 0 255");
	}
}

function Mines_getMineCount(%x,%y)
{
	%check[0] = %x - 1 SPC %y - 1; //upper left
	%check[1] = %x SPC %y - 1; //upper middle
	%check[2] = %x + 1 SPC %y - 1; //upper right
	%check[3] = %x + 1 SPC %y; //middle right
	%check[4] = %x + 1 SPC %y + 1; //lower right
	%check[5] = %x SPC %y + 1; //lower middle
	%check[6] = %x - 1 SPC %y + 1; //lower left
	%check[7] = %x - 1 SPC %y; //middle left
	%mine_count = 0;

	for(%i=0;%i<8;%i++) {
		if(("Mine_Grid" SPC %check[%i]).mine) {
			%mine_count++;
		}
	}

	return %mine_count;
}

function Mines_getButtonCount(%x,%y)
{
	%check[0] = %x - 1 SPC %y - 1; //upper left
	%check[1] = %x SPC %y - 1; //upper middle
	%check[2] = %x + 1 SPC %y - 1; //upper right
	%check[3] = %x + 1 SPC %y; //middle right
	%check[4] = %x + 1 SPC %y + 1; //lower right
	%check[5] = %x SPC %y + 1; //lower middle
	%check[6] = %x - 1 SPC %y + 1; //lower left
	%check[7] = %x - 1 SPC %y; //middle left
	%button_count = 0;

	for(%i=0;%i<8;%i++) {
		if(isObject("Mine_Grid_Button" SPC %check[%i])) {
			%button_count++;
		}
	}

	return %button_count;
}

function Mines_initNumbers() {
	%font_size = 18+($Mines::GridSize-20);
	for(%i=0;%i<$Mines::GridWidth;%i++) {
		for(%j=0;%j<$Mines::GridHeight;%j++) {
			if(!("Mine_Grid" SPC %i SPC %j).mine) {
				%mine_count = Mines_getMineCount(%i,%j);
				if(%mine_count) {
					switch(%mine_count) {
						case 1: %color = "0000cc";
						case 2: %color = "008800";
						case 3: %color = "cc0000";
						case 4: %color = "000000";
						case 5: %color = "996600";
						case 6: %color = "0088cc";
						case 7: %color = "990099";
						case 8: %color = "777777";
					}
					("Mine_Grid_Text" SPC %i SPC %j).setText("<just:center><color:" @ %color @ "><shadow:1:1><shadowcolor:00000022><font:Impact:" @ %font_size @ ">" @ %mine_count);
				}
			}
		}
	}
}


function Mines_clickGrid(%x,%y) {
	%check[0] = %x - 1 SPC %y - 1; //upper left
	%check[1] = %x SPC %y - 1; //upper middle
	%check[2] = %x + 1 SPC %y - 1; //upper right
	%check[3] = %x + 1 SPC %y; //middle right
	%check[4] = %x + 1 SPC %y + 1; //lower right
	%check[5] = %x SPC %y + 1; //lower middle
	%check[6] = %x - 1 SPC %y + 1; //lower left
	%check[7] = %x - 1 SPC %y; //middle left

	%button_count = Mines_getButtonCount(%x,%y);
	%mine_count = Mines_getMineCount(%x,%y);

	for(%i=0;%i<8;%i++) {
		%xs = getWord(%check[%i],0);
		%ys = getWord(%check[%i],1);
		%mine_count_surround = Mines_getMineCount(%xs,%ys);

		if(!%mine_count) {
			if(!%mine_count_surround) {
				if(isObject("Mine_Grid_Button" SPC %check[%i])) {
					("Mine_Grid_Button" SPC %check[%i]).delete();
					$Mines::ButtonCount--;
					Mines_clickGrid(%xs,%ys);
				}
			}
			if(isObject("Mine_Grid_Button" SPC %check[%i])) {
				("Mine_Grid_Button" SPC %check[%i]).delete();
				$Mines::ButtonCount--;
			}
		}
	}

	if(isObject("Mine_Grid_Button" SPC %x SPC %y)) {
		("Mine_Grid_Button" SPC %x SPC %y).delete();
		$Mines::ButtonCount--;
	}

	if(("Mine_Grid" SPC %x SPC %y).mine) {
		Mines_endGame();
	}

	Mines_checkForWin();
}

function Mines_endGame(%win)
{
	cancel($MinesTimer);

	for(%i=0;%i<$Mines::GridWidth;%i++) {
		for(%j=0;%j<$Mines::GridHeight;%j++) {
			if(isObject("Mine_Grid_Button" SPC %i SPC %j)) {
				("Mine_Grid_Button" SPC %i SPC %j).delete();
			}
		}
	}

	if(!%win){
		alxPlay(MinesHitMine);
	}
}

function Mines_markFlag(%x,%y,%state) {
	if(!%state) {
		%ext = $Mines::GridSize SPC $Mines::GridSize;

		%flag = new GuiBitmapButtonCtrl("Mine_Grid_Flag" SPC %x SPC %y) {
			horizSizing = "right";
			vertSizing = "bottom";
			position = "0 0";
			extent = %ext;
			minExtent = %ext;
			enabled = "1";
			visible = "1";
			clipToParent = "1";
			command = "Mines_markFlag(" @ %x @ ", " @ %y @ ", 1);";
			altCommand = "Mines_markFlag(" @ %x @ ", " @ %y @ ", 1);";
			text = "";
			groupNum = "-1";
			buttonType = "PushButton";
			bitmap = "Add-Ons/Client_Mines/images/flag";
			lockAspectRatio = "0";
			alignLeft = "0";
			alignTop = "0";
			overflowImage = "0";
			mKeepCached = "0";
			mColor = "255 255 255 255";
		};
		("Mine_Grid_Button" SPC %x SPC %y).add(%flag);
		$Mines::MinesRemaining--;

		if(("Mine_Grid" SPC %x SPC %y).mine) {
			$Mines::ButtonCount--;
		}
	}
	else {
		("Mine_Grid_Flag" SPC %x SPC %y).delete();
		$Mines::MinesRemaining++;

		if(("Mine_Grid" SPC %x SPC %y).mine) {
			$Mines::ButtonCount++;
		}
	}

	Mines_ScoreText.setText("<just:center><color:0000cc><font:Impact:20>" @ $Mines::MinesRemaining);
	Mines_checkForWin();
}


function Mines_initTimer() {
	cancel($MinesTimer);
	Mines_TimeText.setText("<just:center><color:009900><font:Impact:20>" @ getTimeString(mFloor((getRealTime() - $Mines::StartTime)/1000)));
	$MinesTimer = schedule(1000,0,Mines_initTimer);
}

function Mines_updateSettings() {
	export("$Mines::Settings::*","config/client/mines/prefs.cs",false);
	exec("config/client/mines/prefs.cs");
	$Mines::GridWidth = $Mines::Settings::GridWidth;
	$Mines::GridHeight = $Mines::Settings::GridHeight;
	$Mines::Mines = $Mines::Settings::Mines;
	$Mines::GridSize = $Mines::Settings::GridSize;
	Mines_popDialog(1);
	canvas.popDialog(MinesSettings);
}

function Mines_checkForWin() {
	if(!$Mines::MinesRemaining && !$Mines::ButtonCount) {
		for(%i=0;%i<$Mines::GridWidth;%i++) {
			for(%j=0;%j<$Mines::GridHeight;%j++) {
				if(("Mine_Grid" SPC %i SPC %j).mine) {
					("Mine_Grid" SPC %i SPC %j).setColor("0 255 0 255");
				}
			}
		}

		Mines_endGame(1);
		echo("Win!");
		cancel($MinesTimer);
		alxPlay(MinesWinSound);
	}
}