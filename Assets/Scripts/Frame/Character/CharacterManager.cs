﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class CharacterManager : FrameComponent
{
	protected Dictionary<CHARACTER_TYPE, Type> mCharacterRegisteList;
	protected Dictionary<CHARACTER_TYPE, Dictionary<string, Character>> mCharacterTypeList;    // 角色分类列表
	protected Dictionary<string, Character>		mCharacterList;     // 角色名字索引表
	protected Dictionary<int, Character>		mCharacterGUIDList; // 角色ID索引表
	protected CharacterMyself					mMyself;            // 玩家自己,方便获取
	protected GameObject						mManagerObject;     // 角色管理器物体
	public CharacterManager(string name)
		:base(name)
	{
		mCharacterList = new Dictionary<string, Character>();
		mCharacterTypeList = new Dictionary<CHARACTER_TYPE, Dictionary<string, Character>>();
		mCharacterGUIDList = new Dictionary<int, Character>();
		mCharacterRegisteList = new Dictionary<CHARACTER_TYPE, Type>();
	}
	public override void init()
	{
		mManagerObject = getGameObject(mGameFramework.getGameFrameObject(), "CharacterManager", true);
	}
	public override void destroy()
	{
		base.destroy();
		foreach (var character in mCharacterGUIDList)
		{
			character.Value.destroy();
		}
		mCharacterList = null;
		mCharacterTypeList = null;
		mCharacterGUIDList = null;
		mMyself = null;
	}
	public override void update(float elapsedTime)
	{
		foreach (var item in mCharacterList)
		{
			Character character = item.Value;
			if (character != null && character.getActive())
			{
				character.update(elapsedTime);
			}
		}
	}
	public override void fixedUpdate(float elapsedTime)
	{
		foreach (var item in mCharacterList)
		{
			Character character = item.Value;
			if (character != null && character.getActive())
			{
				character.fixedUpdate(elapsedTime);
			}
		}
	}
	public GameObject getManagerNode() { return mManagerObject; }
	public CharacterMyself getMyself() { return mMyself; }
	public void registeCharacter(Type classType, CHARACTER_TYPE type)
	{
		mCharacterRegisteList.Add(type, classType);
	}
	public Character getCharacter(string name)
	{
		if (!mCharacterList.ContainsKey(name))
		{
			return null;
		}
		return mCharacterList[name];
	}
	public Character getCharacter(int characterID)
	{
		if (!mCharacterGUIDList.ContainsKey(characterID))
		{
			return null;
		}
		return mCharacterGUIDList[characterID];
	}
	public void activeCharacter(int id, bool active)
	{
		activeCharacter(getCharacter(id), active);
	}
	public void activeCharacter(string name, bool active)
	{
		activeCharacter(getCharacter(name), active);
	}
	public void activeCharacter(Character character, bool active)
	{
		character.setActive(active);
	}
	public void getCharacterListByType(CHARACTER_TYPE type, ref Dictionary<string, Character> characterList)
	{
		if (!mCharacterTypeList.ContainsKey(type))
		{
			return;
		}
		characterList = mCharacterTypeList[type];
	}
	public Character createCharacter(string name, CHARACTER_TYPE type, int id, bool createNode)
	{
		if (mCharacterList.ContainsKey(name))
		{
			logError("there is a character named : " + name + "! can not create again!");
			return null;
		}
		if (type == CHARACTER_TYPE.CT_MYSELF)
		{
			if(mMyself != null)
			{
				logError("Myself has exist ! can not create again, name : " + name);
				return null;
			}
		}
		Character newCharacter = createCharacter(type, name);
		// 如果是玩家自己,则记录下来
		if (type == CHARACTER_TYPE.CT_MYSELF)
		{
			mMyself = newCharacter as CharacterMyself;
		}
		if (newCharacter != null)
		{
			// 将角色挂接到管理器下
			if(createNode)
			{
				GameObject charNode = UnityUtility.createObject(newCharacter.getName(), mManagerObject);
				newCharacter.setObject(charNode);
			}
			newCharacter.setID(id);
			newCharacter.init();
			addCharacterToList(newCharacter);
			UnityUtility.notifyIDUsed(id);
		}
		return newCharacter;
	}
	public void destroyCharacter(string name)
	{
		Character character = getCharacter(name);
		if (character != null)
		{
			destroyCharacter(character);
		}
	}
	public void destroyCharacter(int id)
	{
		Character character = getCharacter(id);
		if(character != null)
		{
			destroyCharacter(character);
		}
	}
	public void notifyCharacterIDChanged(int oldID)
	{
		if (mCharacterGUIDList.ContainsKey(oldID))
		{
			Character character = mCharacterGUIDList[oldID];
			mCharacterGUIDList.Remove(oldID);
			mCharacterGUIDList.Add(character.getData().mGUID, character);
		}
	}
	public void notifyCharacterNameChanged(string oldName)
	{
		Character character = null;
		if (mCharacterList.ContainsKey(oldName))
		{
			character = mCharacterList[oldName];
			mCharacterList.Remove(oldName);
			mCharacterList.Add(character.getName(), character);
		}
		if (character != null)
		{
			if (mCharacterTypeList.ContainsKey(character.getType()))
			{
				if (mCharacterTypeList[character.getType()].ContainsKey(oldName))
				{
					mCharacterTypeList[character.getType()].Remove(oldName);
					mCharacterTypeList[character.getType()].Add(character.getName(), character);
				}
			}
		}
	}
	//------------------------------------------------------------------------------------------------------------
	protected void addCharacterToList(Character character)
	{
		if (character == null)
		{
			return;
		}
		// 加入到全部角色列表
		mCharacterList.Add(character.getName(), character);
		// 加入到角色分类列表
		if (!mCharacterTypeList.ContainsKey(character.getType()))
		{
			mCharacterTypeList.Add(character.getType(), new Dictionary<string, Character>());
		}
		mCharacterTypeList[character.getType()].Add(character.getName(), character);
		// 加入ID索引表
		int characterID = character.getData().mGUID;
		if (!mCharacterGUIDList.ContainsKey(characterID))
		{
			mCharacterGUIDList.Add(characterID, character);
		}
		else
		{
			logError("there is a character id : " + characterID + ", can not add again!");
		}
	}
	protected void removeCharacterFromList(Character character)
	{
		if (character == null)
		{
			return;
		}

		// 从全部角色列表中移除
		if (mCharacterList.ContainsKey(character.getName()))
		{
			mCharacterList.Remove(character.getName());
		}
		// 从角色分类列表中移除
		if (mCharacterTypeList.ContainsKey(character.getType()))
		{
			if (mCharacterTypeList[character.getType()].ContainsKey(character.getName()))
			{
				mCharacterTypeList[character.getType()].Remove(character.getName());
			}
		}
		// 从ID索引表中移除
		if (mCharacterGUIDList.ContainsKey(character.getData().mGUID))
		{
			mCharacterGUIDList.Remove(character.getData().mGUID);
		}
	}
	protected void destroyCharacter(Character character)
	{
		removeCharacterFromList(character);
		character.destroy();
		if (mMyself == character)
		{
			mMyself = null;
		}
	}
	protected Character createCharacter(CHARACTER_TYPE type, string name)
	{
		return UnityUtility.createInstance<Character>(mCharacterRegisteList[type], type, name);
	}
}
