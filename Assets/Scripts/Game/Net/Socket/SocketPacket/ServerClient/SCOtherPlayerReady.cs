﻿using System;
using System.Collections;
using System.Collections.Generic;

public class SCOtherPlayerReady : SocketPacket
{
	public BOOL mReady = new BOOL();	// 是否已准备
	public INT mPlayerGUID = new INT();	// 玩家GUID
	public SCOtherPlayerReady(PACKET_TYPE type)
		: base(type) { }
	protected override void fillParams()
	{
		pushParam(mReady);
		pushParam(mPlayerGUID);
	}
	public override void execute()
	{
		CommandCharacterNotifyReady cmd = newCmd(out cmd);
		cmd.mReady = mReady.mValue;
		pushCommand(cmd, mCharacterManager.getCharacter(mPlayerGUID.mValue));
	}
}