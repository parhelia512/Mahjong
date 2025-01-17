﻿using System;
using System.Collections;
using System.Collections.Generic;

public class SCNotifyMahjongEnd : SocketPacket
{
	INTS mCharacterGUIDList = new INTS(GameDefine.MAX_PLAYER_COUNT);
	INTS mMoneyDeltaList = new INTS(GameDefine.MAX_PLAYER_COUNT);
	public SCNotifyMahjongEnd(PACKET_TYPE type)
		: base(type) { }
	protected override void fillParams()
	{
		pushParam(mCharacterGUIDList);
		pushParam(mMoneyDeltaList);
	}
	public override void execute()
	{
		GameScene gameScene = mGameSceneManager.getCurScene();
		if (gameScene.getSceneType() != GAME_SCENE_TYPE.GST_MAHJONG)
		{
			return;
		}
		MahjongScene mahjongScene = gameScene as MahjongScene;
		Room room = mahjongScene.getRoom();
		Dictionary<Character, int> moneyDeltaList = new Dictionary<Character, int>();
		for (int i = 0; i < GameDefine.MAX_PLAYER_COUNT; ++i)
		{
			moneyDeltaList.Add(mCharacterManager.getCharacter(mCharacterGUIDList.mValue[i]), mMoneyDeltaList.mValue[i]);
		}
		CommandRoomEnd cmdEnd = newCmd(out cmdEnd);
		cmdEnd.mMoneyDeltaList = moneyDeltaList;
		pushCommand(cmdEnd, room);
	}
}