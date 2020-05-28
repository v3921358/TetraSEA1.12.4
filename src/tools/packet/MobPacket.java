/*
This file is part of the OdinMS Maple Story Server
Copyright (C) 2008 ~ 2010 Patrick Huy <patrick.huy@frz.cc> 
Matthias Butz <matze@odinms.de>
Jan Christian Meyer <vimes@odinms.de>

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License version 3
as published by the Free Software Foundation. You may not use, modify
or distribute this program under any other version of the
GNU Affero General Public License.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
package tools.packet;

import java.util.Map;
import java.util.List;
import java.awt.Point;

import client.status.MonsterStatus;
import client.status.MonsterStatusEffect;

import constants.GameConstants;
import handling.SendPacketOpcode;
import java.util.Collection;
import server.life.MapleMonster;
import server.life.MobSkill;
import server.movement.LifeMovementFragment;
import tools.MaplePacketCreator;
import tools.data.MaplePacketLittleEndianWriter;

public class MobPacket {

    public static byte[] damageMonster(final int oid, final long damage) {
        MaplePacketLittleEndianWriter mplew = new MaplePacketLittleEndianWriter();

        mplew.writeShort(SendPacketOpcode.DAMAGE_MONSTER.getValue());
        mplew.writeInt(oid);
        mplew.write(0);
        if (damage > Integer.MAX_VALUE) {
            mplew.writeInt(Integer.MAX_VALUE);
        } else {
            mplew.writeInt((int) damage);
        }

        return mplew.getPacket();
    }

    public static byte[] damageFriendlyMob(final MapleMonster mob, final long damage, final boolean display) {
        MaplePacketLittleEndianWriter mplew = new MaplePacketLittleEndianWriter();

        mplew.writeShort(SendPacketOpcode.DAMAGE_MONSTER.getValue());
        mplew.writeInt(mob.getObjectId());
        mplew.write(display ? 1 : 2); //false for when shammos changes map!
        if (damage > Integer.MAX_VALUE) {
            mplew.writeInt(Integer.MAX_VALUE);
        } else { 
            mplew.writeInt((int) damage);
        }
        if (mob.getHp() > Integer.MAX_VALUE) {
            mplew.writeInt((int) (((double) mob.getHp() / mob.getMobMaxHp()) * Integer.MAX_VALUE));
        } else {
            mplew.writeInt((int) mob.getHp());
        }
        if (mob.getMobMaxHp() > Integer.MAX_VALUE) {
            mplew.writeInt(Integer.MAX_VALUE);
        } else {
            mplew.writeInt((int) mob.getMobMaxHp());
        }

        return mplew.getPacket();
    }

    public static byte[] killMonster(final int oid, final int animation) {
        MaplePacketLittleEndianWriter mplew = new MaplePacketLittleEndianWriter();

        mplew.writeShort(SendPacketOpcode.KILL_MONSTER.getValue());
        mplew.writeInt(oid);
        mplew.write(animation); // 0 = dissapear, 1 = fade out, 2+ = special
		if (animation == 4) {
			mplew.writeInt(-1);
		}

        return mplew.getPacket();
    }

    public static byte[] suckMonster(final int oid, final int chr) {
        MaplePacketLittleEndianWriter mplew = new MaplePacketLittleEndianWriter();

        mplew.writeShort(SendPacketOpcode.KILL_MONSTER.getValue());
        mplew.writeInt(oid);
        mplew.write(4);
        mplew.writeInt(chr);

        return mplew.getPacket();
    }

    public static byte[] healMonster(final int oid, final int heal) {
        MaplePacketLittleEndianWriter mplew = new MaplePacketLittleEndianWriter();

        mplew.writeShort(SendPacketOpcode.DAMAGE_MONSTER.getValue());
        mplew.writeInt(oid);
        mplew.write(0);
        mplew.writeInt(-heal);

        return mplew.getPacket();
    }

    public static byte[] showMonsterHP(int oid, int remhppercentage) {
        MaplePacketLittleEndianWriter mplew = new MaplePacketLittleEndianWriter();

        mplew.writeShort(SendPacketOpcode.SHOW_MONSTER_HP.getValue());
        mplew.writeInt(oid);
        mplew.write(remhppercentage);

        return mplew.getPacket();
    }

    public static byte[] showBossHP(final MapleMonster mob) {
        MaplePacketLittleEndianWriter mplew = new MaplePacketLittleEndianWriter();

        mplew.writeShort(SendPacketOpcode.BOSS_ENV.getValue());
        mplew.write(5);
        mplew.writeInt(mob.getId() == 9400589 ? 9300184 : mob.getId()); //hack: MV cant have boss hp bar
        if (mob.getHp() > Integer.MAX_VALUE) {
            mplew.writeInt((int) (((double) mob.getHp() / mob.getMobMaxHp()) * Integer.MAX_VALUE));
        } else {
            mplew.writeInt((int) mob.getHp());
        }
        if (mob.getMobMaxHp() > Integer.MAX_VALUE) {
            mplew.writeInt(Integer.MAX_VALUE);
        } else {
            mplew.writeInt((int) mob.getMobMaxHp());
        }
        mplew.write(mob.getStats().getTagColor());
        mplew.write(mob.getStats().getTagBgColor());

        return mplew.getPacket();
    }

    public static byte[] showBossHP(final int monsterId, final long currentHp, final long maxHp) {
        MaplePacketLittleEndianWriter mplew = new MaplePacketLittleEndianWriter();

        mplew.writeShort(SendPacketOpcode.BOSS_ENV.getValue());
        mplew.write(5);
	mplew.writeInt(monsterId); //has no image
        if (currentHp > Integer.MAX_VALUE) {
            mplew.writeInt((int) (((double) currentHp / maxHp) * Integer.MAX_VALUE));
        } else {
            mplew.writeInt((int) (currentHp <= 0 ? -1 : currentHp));
        }
        if (maxHp > Integer.MAX_VALUE) {
            mplew.writeInt(Integer.MAX_VALUE);
        } else {
            mplew.writeInt((int) maxHp);
        }
        mplew.write(6);
        mplew.write(5);

	//colour legend: (applies to both colours)
	//1 = red, 2 = dark blue, 3 = light green, 4 = dark green, 5 = black, 6 = light blue, 7 = purple

        return mplew.getPacket();
    }

    public static byte[] moveMonster(boolean useskill, int skill, int skill1, int skill2, int skill3, int skill4, int oid, Point startPos, List<LifeMovementFragment> moves) {
        MaplePacketLittleEndianWriter mplew = new MaplePacketLittleEndianWriter();

        mplew.writeShort(SendPacketOpcode.MOVE_MONSTER.getValue());
        mplew.writeInt(oid);
        mplew.writeShort(0); //moveid but always 0
        mplew.write(useskill ? 1 : 0); //?? I THINK
        mplew.write(skill);
        mplew.write(skill1);
        mplew.write(skill2);
        mplew.write(skill3);
        mplew.write(skill4);
        mplew.writeZeroBytes(8); //o.o?
        mplew.writePos(startPos);
        mplew.writeShort(8); //? sometimes 0? sometimes 22? sometimes random numbers?
        mplew.writeShort(1);
        PacketHelper.serializeMovementList(mplew, moves);

        return mplew.getPacket();
    }
    
    public static byte[] spawnMonster(MapleMonster life, int spawnType, int link) {
        MaplePacketLittleEndianWriter mplew = new MaplePacketLittleEndianWriter();

        mplew.writeShort(SendPacketOpcode.SPAWN_MONSTER.getValue());
        mplew.writeInt(life.getObjectId());
        mplew.write(1); // 1 = Control normal, 5 = Control none
        mplew.writeInt(life.getId());
        addMonsterStatus(mplew, life);
        mplew.writePos(life.getTruePosition());
        mplew.write(life.getStance());
        mplew.writeShort(0); // FH
        mplew.writeShort(life.getFh()); // Origin FH
        mplew.write(spawnType);
        if (spawnType == -3 || spawnType >= 0) {
            mplew.writeInt(link);
        }
        mplew.write(life.getCarnivalTeam());
        mplew.writeLong(0); //aftershock - 0xFF at end
		if (GameConstants.GMS) {
			mplew.write(-1);
		}

        return mplew.getPacket();
    }

    public static void addMonsterStatus(MaplePacketLittleEndianWriter mplew, MapleMonster life) {
        if (life.getStati().size() <= 1) {
            life.addEmpty(); //not done yet lulz ok so we add it now for the lulz
        }
		if (GameConstants.GMS) {
			mplew.write(life.getChangedStats() != null ? 1 : 0);
			if (life.getChangedStats() != null) {
				mplew.writeInt(life.getChangedStats().hp > Integer.MAX_VALUE ? Integer.MAX_VALUE : (int)life.getChangedStats().hp);
				mplew.writeInt(life.getChangedStats().mp);
				mplew.writeInt(life.getChangedStats().exp);
				mplew.writeInt(life.getChangedStats().watk);
				mplew.writeInt(life.getChangedStats().matk);
				mplew.writeInt(life.getChangedStats().PDRate);
				mplew.writeInt(life.getChangedStats().MDRate);
				mplew.writeInt(life.getChangedStats().acc);
				mplew.writeInt(life.getChangedStats().eva);
				mplew.writeInt(life.getChangedStats().pushed);
				mplew.writeInt(life.getChangedStats().level);
			}
		}
		final boolean ignore_imm = life.getStati().containsKey(MonsterStatus.WEAPON_DAMAGE_REFLECT) || life.getStati().containsKey(MonsterStatus.MAGIC_DAMAGE_REFLECT);
	Collection<MonsterStatusEffect> buffs = life.getStati().values();
        getLongMask_NoRef(mplew, buffs, ignore_imm); //AFTERSHOCK: extra int
        for (MonsterStatusEffect buff : buffs) {
            if (buff != null && buff.getStati() != MonsterStatus.WEAPON_DAMAGE_REFLECT && buff.getStati() != MonsterStatus.MAGIC_DAMAGE_REFLECT && (!ignore_imm || (buff.getStati() != MonsterStatus.WEAPON_IMMUNITY && buff.getStati() != MonsterStatus.MAGIC_IMMUNITY && buff.getStati() != MonsterStatus.DAMAGE_IMMUNITY))) {
		if (buff.getStati() != MonsterStatus.SUMMON && (buff.getStati() != MonsterStatus.EMPTY_2 || GameConstants.GMS) && (buff.getStati() != MonsterStatus.EMPTY_3 || !GameConstants.GMS)) {
			if (buff.getStati() == MonsterStatus.EMPTY_1 || buff.getStati() == MonsterStatus.EMPTY_2 || buff.getStati() == MonsterStatus.EMPTY_3 || buff.getStati() == MonsterStatus.EMPTY_4 || buff.getStati() == MonsterStatus.EMPTY_5 || buff.getStati() == MonsterStatus.EMPTY_6) {
                mplew.writeShort(Integer.valueOf((int)System.currentTimeMillis()).shortValue()); //wtf
				if (GameConstants.GMS) { //TODO JUMP
					mplew.writeShort(0);
				}
			} else {
				if (GameConstants.GMS) { //TODO JUMP
					mplew.writeInt(buff.getX().intValue());
				} else {
                    mplew.writeShort(buff.getX().shortValue());
				}
			}
                    if (buff.getMobSkill() != null) {
                        mplew.writeShort(buff.getMobSkill().getSkillId());
                        mplew.writeShort(buff.getMobSkill().getSkillLevel());
                    } else if (buff.getSkill() > 0) {
                        mplew.writeInt(buff.getSkill());
                    }
		}

                mplew.writeShort(buff.getStati() == MonsterStatus.HYPNOTIZE ? 40 : (buff.getStati().isEmpty() ? 0 : 1));
				if (buff.getStati() == MonsterStatus.EMPTY_1 || buff.getStati() == MonsterStatus.EMPTY_3) {
					mplew.writeShort(0);
				} else if (buff.getStati() == MonsterStatus.EMPTY_4 || buff.getStati() == MonsterStatus.EMPTY_5) {
					mplew.writeInt(0);
				}
            }
        }
        //wh spawn - 15 zeroes instead of 16, then 98 F4 56 A6 C7 C9 01 28, then 7 zeroes
        //8 -> wh 6, then timestamp, then 28 00, then 6 zeroes
    }

    public static byte[] controlMonster(MapleMonster life, boolean newSpawn, boolean aggro) {
        MaplePacketLittleEndianWriter mplew = new MaplePacketLittleEndianWriter();

        mplew.writeShort(SendPacketOpcode.SPAWN_MONSTER_CONTROL.getValue());
        mplew.write(aggro ? 2 : 1);
        mplew.writeInt(life.getObjectId());
        mplew.write(1); // 1 = Control normal, 5 = Control none
        mplew.writeInt(life.getId());
        addMonsterStatus(mplew, life);
        mplew.writePos(life.getTruePosition());
        mplew.write(life.getStance()); // Bitfield
        mplew.writeShort(0); // FH
        mplew.writeShort(life.getFh()); // Origin FH
        mplew.write(life.isFake() ? -4 : newSpawn ? -2 : -1);
        mplew.write(life.getCarnivalTeam());
        mplew.writeLong(0); //big bang - another int here
		if (GameConstants.GMS) {
			mplew.write(-1);
		}
		
        return mplew.getPacket();
    }

    public static byte[] stopControllingMonster(int oid) {
        MaplePacketLittleEndianWriter mplew = new MaplePacketLittleEndianWriter();

        mplew.writeShort(SendPacketOpcode.SPAWN_MONSTER_CONTROL.getValue());
        mplew.write(0);
        mplew.writeInt(oid);

        return mplew.getPacket();
    }

    public static byte[] makeMonsterReal(MapleMonster life) {
        return spawnMonster(life, -1, 0);
    }

    public static byte[] makeMonsterFake(MapleMonster life) {
        return spawnMonster(life, -4, 0);
    }

    public static byte[] makeMonsterEffect(MapleMonster life, int effect) {
        return spawnMonster(life, effect, 0);
    }

    public static byte[] moveMonsterResponse(int objectid, short moveid, int currentMp, boolean useSkills, int skillId, int skillLevel) {
        MaplePacketLittleEndianWriter mplew = new MaplePacketLittleEndianWriter();

        mplew.writeShort(SendPacketOpcode.MOVE_MONSTER_RESPONSE.getValue());
        mplew.writeInt(objectid);
        mplew.writeShort(moveid);
        mplew.write(useSkills ? 1 : 0);
        mplew.writeShort(currentMp);
        mplew.write(skillId);
        mplew.write(skillLevel);
	mplew.writeInt(0);

        return mplew.getPacket();
    }

    private static void getLongMask_NoRef(MaplePacketLittleEndianWriter mplew, Collection<MonsterStatusEffect> ss, boolean ignore_imm) {
        int[] mask = new int[GameConstants.MAX_BUFFSTAT];
        for (MonsterStatusEffect statup : ss) {
            if (statup != null && statup.getStati() != MonsterStatus.WEAPON_DAMAGE_REFLECT && statup.getStati() != MonsterStatus.MAGIC_DAMAGE_REFLECT && (!ignore_imm || (statup.getStati() != MonsterStatus.WEAPON_IMMUNITY && statup.getStati() != MonsterStatus.MAGIC_IMMUNITY && statup.getStati() != MonsterStatus.DAMAGE_IMMUNITY))) {
                mask[statup.getStati().getPosition() - 1] |= statup.getStati().getValue();
            }
        }
	for (int i = mask.length; i >= 1; i--) {
            mplew.writeInt(mask[i - 1]);
        }
    }

    public static byte[] applyMonsterStatus(final int oid, final MonsterStatus mse, int x, MobSkill skil) {
        MaplePacketLittleEndianWriter mplew = new MaplePacketLittleEndianWriter();

        mplew.writeShort(SendPacketOpcode.APPLY_MONSTER_STATUS.getValue());
        mplew.writeInt(oid);
        PacketHelper.writeSingleMask(mplew, mse);

		if (GameConstants.GMS) { //TODO JUMP
			mplew.writeInt(x);
		} else {
			mplew.writeShort(x);
		}
        mplew.writeShort(skil.getSkillId());
        mplew.writeShort(skil.getSkillLevel());
        mplew.writeShort(mse.isEmpty() ? 1 : 0); // might actually be the buffTime but it's not displayed anywhere
        mplew.writeShort(0); // delay in ms
        mplew.write(1); // size
        mplew.write(1); // ? v97

        return mplew.getPacket();
    }

    public static byte[] applyMonsterStatus(final MapleMonster mons, final MonsterStatusEffect ms) {
        MaplePacketLittleEndianWriter mplew = new MaplePacketLittleEndianWriter();
        mplew.writeShort(SendPacketOpcode.APPLY_MONSTER_STATUS.getValue());
        mplew.writeInt(mons.getObjectId());
        //aftershock extra int here
        PacketHelper.writeSingleMask(mplew, ms.getStati());
		if (GameConstants.GMS) { //TODO JUMP
			mplew.writeInt(ms.getX().intValue());
		} else {
            mplew.writeShort(ms.getX().shortValue());
		}
        if (ms.isMonsterSkill()) {
            mplew.writeShort(ms.getMobSkill().getSkillId());
            mplew.writeShort(ms.getMobSkill().getSkillLevel());
        } else if (ms.getSkill() > 0) {
            mplew.writeInt(ms.getSkill());
        }
        mplew.writeShort(ms.getStati().isEmpty() ? 1 : 0); // might actually be the buffTime but it's not displayed anywhere
        mplew.writeShort(0); // delay in ms
        mplew.write(1); // size
        mplew.write(1); // ? v97

        return mplew.getPacket();
    }

    public static byte[] applyMonsterStatus(final MapleMonster mons, final List<MonsterStatusEffect> mse) {
	if (mse.size() <= 0 || mse.get(0) == null) {
	    return MaplePacketCreator.enableActions();
	}
        MaplePacketLittleEndianWriter mplew = new MaplePacketLittleEndianWriter();

        mplew.writeShort(SendPacketOpcode.APPLY_MONSTER_STATUS.getValue());
        mplew.writeInt(mons.getObjectId());
        final MonsterStatusEffect ms = mse.get(0);
        if (ms.getStati() == MonsterStatus.POISON) { //stack ftw
	    PacketHelper.writeSingleMask(mplew, MonsterStatus.EMPTY);
            mplew.write(mse.size());
            for (MonsterStatusEffect m : mse) {
                mplew.writeInt(m.getFromID()); //character ID
                if (m.isMonsterSkill()) {
                    mplew.writeShort(m.getMobSkill().getSkillId());
                    mplew.writeShort(m.getMobSkill().getSkillLevel());
                } else if (m.getSkill() > 0) {
                    mplew.writeInt(m.getSkill());
                }
                mplew.writeInt(m.getX()); //dmg
                mplew.writeLong(1000); //delay -> tick count
                mplew.writeInt(5); //buff time ?
            }
            mplew.writeShort(300); // delay in ms
            mplew.write(1); // size
	} else {
	    PacketHelper.writeSingleMask(mplew, ms.getStati());
			if (GameConstants.GMS) { //TODO JUMP
				mplew.writeInt(ms.getX().intValue());
			} else {
                mplew.writeShort(ms.getX().shortValue());
			}
            if (ms.isMonsterSkill()) {
                mplew.writeShort(ms.getMobSkill().getSkillId());
                mplew.writeShort(ms.getMobSkill().getSkillLevel());
            } else if (ms.getSkill() > 0) {
                mplew.writeInt(ms.getSkill());
            }
            mplew.writeShort(0); // might actually be the buffTime but it's not displayed anywhere
            mplew.writeShort(0); // delay in ms
            mplew.write(1); // size
            mplew.write(1); // ? v97
        }


        return mplew.getPacket();
    }

    public static byte[] applyMonsterStatus(final int oid, final Map<MonsterStatus, Integer> stati, final List<Integer> reflection, MobSkill skil) {
        MaplePacketLittleEndianWriter mplew = new MaplePacketLittleEndianWriter();

        mplew.writeShort(SendPacketOpcode.APPLY_MONSTER_STATUS.getValue());
        mplew.writeInt(oid);
        PacketHelper.writeMask(mplew, stati.keySet());

        for (Map.Entry<MonsterStatus, Integer> mse : stati.entrySet()) {
			if (GameConstants.GMS) { //TODO JUMP
				mplew.writeInt(mse.getValue().intValue());
			} else {
                mplew.writeShort(mse.getValue().shortValue());
			}
            mplew.writeShort(skil.getSkillId());
            mplew.writeShort(skil.getSkillLevel());
            mplew.writeShort(0); // might actually be the buffTime but it's not displayed anywhere
        }
        for (Integer ref : reflection) {
            mplew.writeInt(ref);
        }
        mplew.writeLong(0);
        mplew.writeShort(0); // delay in ms

        int size = stati.size(); // size
        if (reflection.size() > 0) {
            size /= 2; // This gives 2 buffs per reflection but it's really one buff
        }
        mplew.write(size); // size
        mplew.write(1); // ? v97

        return mplew.getPacket();
    }

    public static byte[] cancelMonsterStatus(int oid, MonsterStatus stat) {
        MaplePacketLittleEndianWriter mplew = new MaplePacketLittleEndianWriter();

        mplew.writeShort(SendPacketOpcode.CANCEL_MONSTER_STATUS.getValue());
        mplew.writeInt(oid);
        PacketHelper.writeSingleMask(mplew, stat);
        mplew.write(1); // reflector is 3~!??
        mplew.write(2); // ? v97

        return mplew.getPacket();
    }
	
    public static byte[] cancelPoison(int oid, MonsterStatusEffect m) {
        MaplePacketLittleEndianWriter mplew = new MaplePacketLittleEndianWriter();

        mplew.writeShort(SendPacketOpcode.CANCEL_MONSTER_STATUS.getValue());
        mplew.writeInt(oid);
        PacketHelper.writeSingleMask(mplew, MonsterStatus.EMPTY);
		mplew.writeInt(0);
		mplew.writeInt(1); //size probably
        mplew.writeInt(m.getFromID()); //character ID
        if (m.isMonsterSkill()) {
            mplew.writeShort(m.getMobSkill().getSkillId());
            mplew.writeShort(m.getMobSkill().getSkillLevel());
        } else if (m.getSkill() > 0) {
            mplew.writeInt(m.getSkill());
        }
        mplew.write(3); // ? v97

        return mplew.getPacket();
    }

    public static byte[] talkMonster(int oid, int itemId, String msg) {
        MaplePacketLittleEndianWriter mplew = new MaplePacketLittleEndianWriter();

        mplew.writeShort(SendPacketOpcode.TALK_MONSTER.getValue());
        mplew.writeInt(oid);
        mplew.writeInt(500); //?
        mplew.writeInt(itemId);
        mplew.write(itemId <= 0 ? 0 : 1);
        mplew.write(msg == null || msg.length() <= 0 ? 0 : 1);
        if (msg != null && msg.length() > 0) {
            mplew.writeMapleAsciiString(msg);
        }
        mplew.writeInt(1); //?

        return mplew.getPacket();
    }

    public static byte[] removeTalkMonster(int oid) {
        MaplePacketLittleEndianWriter mplew = new MaplePacketLittleEndianWriter();

        mplew.writeShort(SendPacketOpcode.REMOVE_TALK_MONSTER.getValue());
        mplew.writeInt(oid);
        return mplew.getPacket();
    }
}
