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
package handling.channel.handler;

import constants.GameConstants;
import java.awt.Point;
import java.util.ArrayList;
import java.util.List;

import server.maps.AnimatedMapleMapObject;
import server.movement.*;
import tools.FileoutputUtil;
import tools.data.LittleEndianAccessor;

public class MovementParse {

    //1 = player, 2 = mob, 3 = pet, 4 = summon, 5 = dragon
    public static final List<LifeMovementFragment> parseMovement(final LittleEndianAccessor lea, int kind) {
        final List<LifeMovementFragment> res = new ArrayList<LifeMovementFragment>();
        final byte numCommands = lea.readByte();

        for (byte i = 0; i < numCommands; i++) {
            final byte command = lea.readByte();
            switch (command) {
                //-2 = 12 02 0D FE 00 00 00 00 07 1E 00 00 BE
                //FE 12 02 0C FE 00 00 27 00 05 00 00
                //-3 = 12 02 0D FE 00 00 00 00 07 1E 00 00 F2
                //FD 12 02 0C FE 00 00 3F 00 05 00 00
                case -1: // Bounce movement?
                {
                    final short xpos = lea.readShort();
                    final short ypos = lea.readShort();
                    final short unk = lea.readShort();
                    final short fh = lea.readShort();
                    final byte newstate = lea.readByte();
                    final short duration = lea.readShort();
                    final BounceMovement bm = new BounceMovement(command, new Point(xpos, ypos), duration, newstate);
                    bm.setFH(fh);
                    bm.setUnk(unk);
                    res.add(bm);
                    break;
                }
                case 0: // normal move
                case 37:
                case 38:
                case 39:
                case 40:
                case 41:
                case 42: {
                    final short xpos = lea.readShort();
                    final short ypos = lea.readShort();
                    final short xwobble = lea.readShort();
                    final short ywobble = lea.readShort();
                    final short unk = lea.readShort();
                    final short xoffset = lea.readShort();
                    final short yoffset = lea.readShort();
                    final byte newstate = lea.readByte();
                    final short duration = lea.readShort();
                    final AbsoluteLifeMovement alm = new AbsoluteLifeMovement(command, new Point(xpos, ypos), duration, newstate);
                    alm.setUnk(unk);
                    alm.setPixelsPerSecond(new Point(xwobble, ywobble));
                    alm.setOffset(new Point(xoffset, yoffset));
                    // log.trace("Move to {},{} command {} wobble {},{} ? {} state {} duration {}", new Object[] { xpos,
                    // xpos, command, xwobble, ywobble, newstate, duration });
                    res.add(alm);
                    break;
                }
                case 33:
                case 34:
                case 36:
                case 1:
                case 2: { // Float
                    final short xmod = lea.readShort();
                    final short ymod = lea.readShort();
                    final byte newstate = lea.readByte();
                    final short duration = lea.readShort();
                    final RelativeLifeMovement rlm = new RelativeLifeMovement(command, new Point(xmod, ymod), duration, newstate);
                    res.add(rlm);
                    // log.trace("Relative move {},{} state {}, duration {}", new Object[] { xmod, ymod, newstate,
                    // duration });
                    break;
                }
				case 5:
				case 6: // assaulter
				case 7: //same structure
				case 16:
                case 17:
                case 20:
				case 19: // Soaring?
                case 18: { // Float
					if ((GameConstants.GMS && command == 19) || (!GameConstants.GMS && command == 18)) {
						final short xpos = lea.readShort();
						final short ypos = lea.readShort();
						final short unk = lea.readShort();
						final short fh = lea.readShort();
						final byte newstate = lea.readByte();
						final short duration = lea.readShort();
						final BounceMovement bm = new BounceMovement(command, new Point(xpos, ypos), duration, newstate);
						bm.setFH(fh);
						bm.setUnk(unk);
						res.add(bm);
					} else if ((GameConstants.GMS && command == 17) || (!GameConstants.GMS && command == 16) || (!GameConstants.GMS && command == 20)) {
						final byte newstate = lea.readByte();
						final short unk = lea.readShort();
						final AranMovement am = new AranMovement(command, new Point(0, 0), unk, newstate);

						res.add(am);
					} else if ((GameConstants.GMS && command == 20) || (!GameConstants.GMS && command == 19) || (GameConstants.GMS && command == 18) || (!GameConstants.GMS && command == 17)) {
						final short xmod = lea.readShort();
						final short ymod = lea.readShort();
						final byte newstate = lea.readByte();
						final short duration = lea.readShort();
						final RelativeLifeMovement rlm = new RelativeLifeMovement(command, new Point(xmod, ymod), duration, newstate);
						res.add(rlm);
					} else if ((!GameConstants.GMS && command == 5) || (!GameConstants.GMS && command == 7) || (GameConstants.GMS && command == 6)) {
						final short xpos = lea.readShort();
						final short ypos = lea.readShort();
						final short xwobble = lea.readShort();
						final short ywobble = lea.readShort();
						final byte newstate = lea.readByte();
						final TeleportMovement tm = new TeleportMovement(command, new Point(xpos, ypos), newstate);
						tm.setPixelsPerSecond(new Point(xwobble, ywobble));
						res.add(tm);
					} else {
						final short xpos = lea.readShort();
						final short ypos = lea.readShort();
						final short xwobble = lea.readShort();
						final short ywobble = lea.readShort();
						final short unk = lea.readShort();
						final short xoffset = lea.readShort();
						final short yoffset = lea.readShort();
						final byte newstate = lea.readByte();
						final short duration = lea.readShort();
						final AbsoluteLifeMovement alm = new AbsoluteLifeMovement(command, new Point(xpos, ypos), duration, newstate);
						alm.setUnk(unk);
						alm.setPixelsPerSecond(new Point(xwobble, ywobble));
						alm.setOffset(new Point(xoffset, yoffset));
						// log.trace("Move to {},{} command {} wobble {},{} ? {} state {} duration {}", new Object[] { xpos,
						// xpos, command, xwobble, ywobble, newstate, duration });
						res.add(alm);
					}
					break;
				}
				case 100:
                case 101: //wut
                case 3: // rush
                case 4: // tele... -.-
                case 8: // assassinate
                {
                    final short xpos = lea.readShort();
                    final short ypos = lea.readShort();
                    final short xwobble = lea.readShort();
                    final short ywobble = lea.readShort();
                    final byte newstate = lea.readByte();
                    final TeleportMovement tm = new TeleportMovement(command, new Point(xpos, ypos), newstate);
                    tm.setPixelsPerSecond(new Point(xwobble, ywobble));
                    res.add(tm);
                    break;
                }
                case 10: 
				case 11: {
                    if ((GameConstants.GMS && command == 10) || (!GameConstants.GMS && command == 11)) {
                        final short xpos = lea.readShort();
                        final short ypos = lea.readShort();
                        final short unk = lea.readShort();
                        final byte newstate = lea.readByte();
                        final short duration = lea.readShort();
                        final ChairMovement cm = new ChairMovement(command, new Point(xpos, ypos), duration, newstate);
                        cm.setUnk(unk);
                        res.add(cm);
                    } else {
                        res.add(new ChangeEquipSpecialAwesome(command, lea.readByte()));
                    }
                    break;
                }
				case 9:
                case 12: // chair ???
                {
                    final short xpos = lea.readShort();
                    final short ypos = lea.readShort();
                    final short unk = lea.readShort();
                    final byte newstate = lea.readByte();
                    final short duration = lea.readShort();
                    final ChairMovement cm = new ChairMovement(command, new Point(xpos, ypos), duration, newstate);
                    cm.setUnk(unk);
                    res.add(cm);
                    break;
                }
                case 25: //?
                case 26: //?
                case 27: //?
                case 28: //?
                case 29: //? <- has no offsets
                case 30:
                case 31:
                case 35: // i think, well in gms anyway
				case 21:
                case 22:
                case 23: // Aran Combat Step
                case 24: {
                    final byte newstate = lea.readByte();
                    final short unk = lea.readShort();
                    final AranMovement am = new AranMovement(command, new Point(0, 0), unk, newstate);

                    res.add(am);
                    break;
                }
				case 13:
                case 14: { // Jump Down
					if ((GameConstants.GMS && command == 14) || (!GameConstants.GMS && command == 13)) {
						final short xpos = lea.readShort();
						final short ypos = lea.readShort();
						final short xwobble = lea.readShort();
						final short ywobble = lea.readShort();
						final short unk = lea.readShort();
						final short fh = lea.readShort();
						final short xoffset = lea.readShort();
						final short yoffset = lea.readShort();
						final byte newstate = lea.readByte();
						final short duration = lea.readShort();
						final JumpDownMovement jdm = new JumpDownMovement(command, new Point(xpos, ypos), duration, newstate);
						jdm.setUnk(unk);
						jdm.setPixelsPerSecond(new Point(xwobble, ywobble));
						jdm.setOffset(new Point(xoffset, yoffset));
						jdm.setFH(fh);

						res.add(jdm);
					} else if (GameConstants.GMS && command == 13) {
						final short xpos = lea.readShort();
						final short ypos = lea.readShort();
						final short unk = lea.readShort();
						final byte newstate = lea.readByte();
						final short duration = lea.readShort();
						final ChairMovement cm = new ChairMovement(command, new Point(xpos, ypos), duration, newstate);
						cm.setUnk(unk);
						res.add(cm);
					} else {
						final short xmod = lea.readShort();
						final short ymod = lea.readShort();
						final byte newstate = lea.readByte();
						final short duration = lea.readShort();
						final RelativeLifeMovement rlm = new RelativeLifeMovement(command, new Point(xmod, ymod), duration, newstate);
						res.add(rlm);
					}
                    break;
                }
                case 15: { // Float
					if (GameConstants.GMS) {
						final short xmod = lea.readShort();
						final short ymod = lea.readShort();
						final byte newstate = lea.readByte();
						final short duration = lea.readShort();
						final RelativeLifeMovement rlm = new RelativeLifeMovement(command, new Point(xmod, ymod), duration, newstate);
						res.add(rlm);
					} else {
						final short xpos = lea.readShort();
						final short ypos = lea.readShort();
						final short xwobble = lea.readShort();
						final short ywobble = lea.readShort();
						final short unk = lea.readShort();
						final short xoffset = lea.readShort();
						final short yoffset = lea.readShort();
						final byte newstate = lea.readByte();
						final short duration = lea.readShort();
						final AbsoluteLifeMovement alm = new AbsoluteLifeMovement(command, new Point(xpos, ypos), duration, newstate);
						alm.setUnk(unk);
						alm.setPixelsPerSecond(new Point(xwobble, ywobble));
						alm.setOffset(new Point(xoffset, yoffset));
						// log.trace("Move to {},{} command {} wobble {},{} ? {} state {} duration {}", new Object[] { xpos,
						// xpos, command, xwobble, ywobble, newstate, duration });
						res.add(alm);
					}
					break;
                }
                case 32: { //?... 00 00 7A 03 0F 02 64 01 00 00 0F 00 04 5A 00
                    final short unk = lea.readShort(); //always 0?
                    final short xpos = lea.readShort(); //not xpos
                    final short ypos = lea.readShort(); //not ypos
                    final short xwobble = lea.readShort();
                    final short ywobble = lea.readShort();
                    final short fh = lea.readShort();
                    final byte newstate = lea.readByte();
                    final short duration = lea.readShort();
                    final UnknownMovement um = new UnknownMovement(command, new Point(xpos, ypos), duration, newstate);
                    um.setUnk(unk);
                    um.setPixelsPerSecond(new Point(xwobble, ywobble));
                    um.setFH(fh);

                    res.add(um);
                    break;
                }
                default:
                    FileoutputUtil.log(FileoutputUtil.Movement_Log, "Kind movement: " + kind + ", Remaining : " + (numCommands - res.size()) + " New type of movement ID : " + command + ", packet : " + lea.toString(true));
                    return null;
            }
        }
        if (numCommands != res.size()) {
//	    System.out.println("error in movement");
            return null; // Probably hack
        }
        return res;
    }

    public static final void updatePosition(final List<LifeMovementFragment> movement, final AnimatedMapleMapObject target, final int yoffset) {
        if (movement == null) {
            return;
        }
        for (final LifeMovementFragment move : movement) {
            if (move instanceof LifeMovement) {
                if (move instanceof AbsoluteLifeMovement) {
                    final Point position = ((LifeMovement) move).getPosition();
                    position.y += yoffset;
                    target.setPosition(position);
                }
                target.setStance(((LifeMovement) move).getNewstate());
            }
        }
    }
}
