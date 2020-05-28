package server;

import client.SkillFactory;
import client.inventory.MapleInventoryIdentifier;
import client.messages.commands.SuperGMCommand;
import constants.BattleConstants;
import constants.ServerConstants;
import handling.MapleServerHandler;
import handling.channel.ChannelServer;
import handling.channel.MapleGuildRanking;
import handling.login.LoginServer;
import handling.cashshop.CashShopServer;
import handling.login.LoginInformationProvider;
import handling.world.World;
import java.sql.SQLException;
import database.DatabaseConnection;
import handling.world.family.MapleFamily;
import handling.world.guild.MapleGuild;
import java.sql.PreparedStatement;
import server.Timer.*;
import server.events.MapleOxQuizFactory;
import server.life.MapleLifeFactory;
import server.life.MapleMonsterInformationProvider;
import server.life.MobSkillFactory;
import server.life.PlayerNPC;
import server.quest.MapleQuest;
import java.util.concurrent.atomic.AtomicInteger;

public class Start {

    public static long startTime = System.currentTimeMillis();
    public static final Start instance = new Start();
	public static AtomicInteger CompletedLoadingThreads = new AtomicInteger(0);

    public void run() throws InterruptedException {

        if (Boolean.parseBoolean(ServerProperties.getProperty("net.sf.odinms.world.admin")) || ServerConstants.Use_Localhost) {
	    ServerConstants.Use_Fixed_IV = false;
            System.out.println("[!!! Admin Only Mode Active !!!]");
        }

        try {
            final PreparedStatement ps = DatabaseConnection.getConnection().prepareStatement("UPDATE accounts SET loggedin = 0");
            ps.executeUpdate();
            ps.close();
        } catch (SQLException ex) {
            throw new RuntimeException("[EXCEPTION] Please check if the SQL server is active.");
        }

        System.out.println("[" + ServerProperties.getProperty("net.sf.odinms.login.serverName") + "] Revision: " + SuperGMCommand.Rev.getRevision());
        World.init();
        WorldTimer.getInstance().start();
	EtcTimer.getInstance().start();
        MapTimer.getInstance().start();
        CloneTimer.getInstance().start();
        EventTimer.getInstance().start();
        BuffTimer.getInstance().start();
	PingTimer.getInstance().start();
        LoadingThread WorldLoader = new LoadingThread(new Runnable() {

            public void run() {
                MapleGuildRanking.getInstance().load();
                MapleGuild.loadAll(); //(this);
            }
        }, "WorldLoader", this);
        LoadingThread FamilyLoader = new LoadingThread(new Runnable() {

            public void run() {
                MapleFamily.loadAll(); //(this);
            }
        }, "FamilyLoader", this);
        LoadingThread QuestLoader = new LoadingThread(new Runnable() {

            public void run() {
                MapleLifeFactory.loadQuestCounts();
                MapleQuest.initQuests();
            }
        }, "QuestLoader", this);
        LoadingThread ProviderLoader = new LoadingThread(new Runnable() {

            public void run() {
                MapleItemInformationProvider.getInstance().runEtc();
			}
		}, "ProviderLoader", this);
        LoadingThread MonsterLoader = new LoadingThread(new Runnable() {

            public void run() {
                MapleMonsterInformationProvider.getInstance().load();
                BattleConstants.init();
            }
        }, "MonsterLoader", this);
        LoadingThread ItemLoader = new LoadingThread(new Runnable() {

            public void run() {
                MapleItemInformationProvider.getInstance().runItems();
            }
        }, "ItemLoader", this);
        LoadingThread SkillFactoryLoader = new LoadingThread(new Runnable() {

            public void run() {
                SkillFactory.load();
            }
        }, "SkillFactoryLoader", this);
        LoadingThread BasicLoader = new LoadingThread(new Runnable() {

            public void run() {
				LoginInformationProvider.getInstance();
                RandomRewards.load();
                MapleOxQuizFactory.getInstance();
                MapleCarnivalFactory.getInstance();
                MobSkillFactory.getInstance();
				SpeedRunner.loadSpeedRuns();
				MTSStorage.load();
            }
        }, "BasicLoader", this);
        LoadingThread MIILoader = new LoadingThread(new Runnable() {

            public void run() {
		MapleInventoryIdentifier.getInstance();
            }
        }, "MIILoader", this);
        LoadingThread CashItemLoader = new LoadingThread(new Runnable() {

            public void run() {
                CashItemFactory.getInstance().initialize();
            }
        }, "CashItemLoader", this);

        LoadingThread[] LoadingThreads = {WorldLoader, FamilyLoader, QuestLoader, ProviderLoader, SkillFactoryLoader,
            BasicLoader, CashItemLoader, MIILoader, MonsterLoader, ItemLoader};

        for (Thread t : LoadingThreads) {
            t.start();
        }
        synchronized (this) {
            wait();
        }
        while (CompletedLoadingThreads.incrementAndGet() != LoadingThreads.length) {
            synchronized (this) {
                wait();
            }
        }

        MapleServerHandler.initiate();
        System.out.println("[Loading Login]");
        LoginServer.run_startup_configurations();
        System.out.println("[Login Initialized]");

        System.out.println("[Loading Channel]");
        ChannelServer.startChannel_Main();
        System.out.println("[Channel Initialized]");

        System.out.println("[Loading CS]");
        CashShopServer.run_startup_configurations();
        System.out.println("[CS Initialized]");

        //threads.
        CheatTimer.getInstance().register(AutobanManager.getInstance(), 60000);
        Runtime.getRuntime().addShutdownHook(new Thread(new Shutdown()));
		World.registerRespawn();
	//ChannelServer.getInstance(1).getMapFactory().getMap(910000000).spawnRandDrop(); //start it off
		ShutdownServer.registerMBean();
		ServerConstants.registerMBean();
        PlayerNPC.loadAll();// touch - so we see database problems early...
		MapleMonsterInformationProvider.getInstance().addExtra();
		LoginServer.setOn(); //now or later
        System.out.println("[Fully Initialized in " + ((System.currentTimeMillis() - startTime) / 1000) + " seconds]");
        RankingWorker.run();
    }

    public static class Shutdown implements Runnable {

        @Override
        public void run() {
            ShutdownServer.getInstance().run();
            ShutdownServer.getInstance().run();
        }
    }

    public static void main(final String args[]) throws InterruptedException {
        instance.run();
    }

    private static class LoadingThread extends Thread {

        protected String LoadingThreadName;

        private LoadingThread(Runnable r, String t, Object o) {
            super(new NotifyingRunnable(r, o, t));
            LoadingThreadName = t;
        }

        @Override
        public synchronized void start() {
            System.out.println("[Loading...] Started " + LoadingThreadName + " Thread");
            super.start();
        }
    }

    private static class NotifyingRunnable implements Runnable {

        private String LoadingThreadName;
        private long StartTime;
        private Runnable WrappedRunnable;
        private final Object ToNotify;

        private NotifyingRunnable(Runnable r, Object o, String name) {
            WrappedRunnable = r;
            ToNotify = o;
            LoadingThreadName = name;
        }

        public void run() {
            StartTime = System.currentTimeMillis();
            WrappedRunnable.run();
            System.out.println("[Loading Completed] " + LoadingThreadName + " | Completed in " + (System.currentTimeMillis() - StartTime) + " Milliseconds. (" + CompletedLoadingThreads.get() + "/10)");
            synchronized (ToNotify) {
                ToNotify.notify();
            }
        }
    }
}
