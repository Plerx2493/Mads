﻿// <auto-generated />
using System;
using MADS.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace MADS.Migrations
{
    [DbContext(typeof(MadsContext))]
    partial class MadsContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("MADS.Entities.GuildConfigDbEntity", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("id");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<ulong>("Id"));

                    b.Property<ulong>("GuildId")
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("discordId");

                    b.Property<string>("Prefix")
                        .IsRequired()
                        .HasMaxLength(5)
                        .HasColumnType("varchar(5)")
                        .HasColumnName("prefix");

                    b.Property<bool>("StarboardActive")
                        .HasColumnType("tinyint(1)")
                        .HasColumnName("starboardEnabled");

                    b.Property<ulong?>("StarboardChannelId")
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("starboardChannel");

                    b.Property<ulong?>("StarboardEmojiId")
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("starboardEmojiId");

                    b.Property<string>("StarboardEmojiName")
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)")
                        .HasColumnName("starboardEmojiName");

                    b.Property<int?>("StarboardThreshold")
                        .HasColumnType("int")
                        .HasColumnName("starboardThreshold");

                    b.HasKey("Id");

                    b.HasIndex("GuildId")
                        .IsUnique();

                    b.ToTable("Configs");
                });

            modelBuilder.Entity("MADS.Entities.GuildDbEntity", b =>
                {
                    b.Property<ulong>("Id")
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("id");

                    b.HasKey("Id");

                    b.ToTable("guilds", (string)null);
                });

            modelBuilder.Entity("MADS.Entities.IncidentDbEntity", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("id");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<ulong>("Id"));

                    b.Property<DateTimeOffset>("CreationTimeStamp")
                        .HasColumnType("datetime(6)");

                    b.Property<ulong>("GuildId")
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("guild_id");

                    b.Property<ulong>("ModeratorId")
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("moderator_id");

                    b.Property<string>("Reason")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("reason");

                    b.Property<ulong>("TargetId")
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("target_id");

                    b.HasKey("Id");

                    b.HasIndex("GuildId");

                    b.HasIndex("TargetId");

                    b.ToTable("Incidents");
                });

            modelBuilder.Entity("MADS.Entities.QuoteDbEntity", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("id");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<ulong>("Id"));

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("varchar(1000)")
                        .HasColumnName("content");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)")
                        .HasColumnName("timestamp");

                    b.Property<ulong>("GuildId")
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("discordGuildId");

                    b.Property<ulong>("QuotedUserId")
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("quotedUserId");

                    b.Property<ulong>("UserId")
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("UserId");

                    b.HasKey("Id");

                    b.HasIndex("GuildId");

                    b.ToTable("Quotes");
                });

            modelBuilder.Entity("MADS.Entities.ReminderDbEntity", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("id");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<ulong>("Id"));

                    b.Property<ulong>("ChannelId")
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("channelId");

                    b.Property<DateTime>("CreationTime")
                        .HasColumnType("datetime(6)")
                        .HasColumnName("creationTime");

                    b.Property<DateTime>("ExecutionTime")
                        .HasColumnType("datetime(6)")
                        .HasColumnName("executionTime");

                    b.Property<bool>("IsPrivate")
                        .HasColumnType("tinyint(1)")
                        .HasColumnName("isPrivate");

                    b.Property<ulong>("MentionedChannel")
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("mentionedChannel");

                    b.Property<ulong>("MentionedMessage")
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("MentionedMessage");

                    b.Property<ulong>("MessageId")
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("messageId");

                    b.Property<string>("ReminderText")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("reminderText");

                    b.Property<ulong>("UserId")
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("userId");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Reminders");
                });

            modelBuilder.Entity("MADS.Entities.StarboardMessageDbEntity", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("id");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<ulong>("Id"));

                    b.Property<ulong>("DiscordChannelId")
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("discordChannelId");

                    b.Property<ulong>("DiscordGuildId")
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("discordGuildId");

                    b.Property<ulong>("DiscordMessageId")
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("discordMessageId");

                    b.Property<ulong>("StarboardChannelId")
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("starboardChannelId");

                    b.Property<ulong>("StarboardGuildId")
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("starboardGuildId");

                    b.Property<ulong>("StarboardMessageId")
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("starboardMessageId");

                    b.Property<int>("Stars")
                        .HasColumnType("int")
                        .HasColumnName("starCount");

                    b.HasKey("Id");

                    b.ToTable("Starboard");
                });

            modelBuilder.Entity("MADS.Entities.UserDbEntity", b =>
                {
                    b.Property<ulong>("Id")
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("id");

                    b.Property<string>("PreferedLanguage")
                        .HasColumnType("longtext")
                        .HasColumnName("prefered_language");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("username");

                    b.HasKey("Id");

                    b.ToTable("users", (string)null);
                });

            modelBuilder.Entity("MADS.Entities.VoiceAlert", b =>
                {
                    b.Property<ulong>("AlertId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("id");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<ulong>("AlertId"));

                    b.Property<ulong>("ChannelId")
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("channel_id");

                    b.Property<ulong>("GuildId")
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("guild_id");

                    b.Property<bool>("IsRepeatable")
                        .HasColumnType("tinyint(1)")
                        .HasColumnName("is_repeatable");

                    b.Property<DateTimeOffset?>("LastAlert")
                        .HasColumnType("datetime(6)")
                        .HasColumnName("last_alert");

                    b.Property<TimeSpan?>("MinTimeBetweenAlerts")
                        .HasColumnType("time(6)")
                        .HasColumnName("time_between");

                    b.Property<ulong>("UserId")
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("user_id");

                    b.HasKey("AlertId");

                    b.HasIndex("UserId");

                    b.ToTable("VoiceAlerts");
                });

            modelBuilder.Entity("MADS.Entities.GuildConfigDbEntity", b =>
                {
                    b.HasOne("MADS.Entities.GuildDbEntity", "Guild")
                        .WithOne("Settings")
                        .HasForeignKey("MADS.Entities.GuildConfigDbEntity", "GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Guild");
                });

            modelBuilder.Entity("MADS.Entities.IncidentDbEntity", b =>
                {
                    b.HasOne("MADS.Entities.GuildDbEntity", "Guild")
                        .WithMany("Incidents")
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MADS.Entities.UserDbEntity", "TargetUser")
                        .WithMany("Incidents")
                        .HasForeignKey("TargetId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Guild");

                    b.Navigation("TargetUser");
                });

            modelBuilder.Entity("MADS.Entities.QuoteDbEntity", b =>
                {
                    b.HasOne("MADS.Entities.GuildDbEntity", "Guild")
                        .WithMany("Quotes")
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Guild");
                });

            modelBuilder.Entity("MADS.Entities.ReminderDbEntity", b =>
                {
                    b.HasOne("MADS.Entities.UserDbEntity", "User")
                        .WithMany("Reminders")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("MADS.Entities.VoiceAlert", b =>
                {
                    b.HasOne("MADS.Entities.UserDbEntity", "User")
                        .WithMany("VoiceAlerts")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("MADS.Entities.GuildDbEntity", b =>
                {
                    b.Navigation("Incidents");

                    b.Navigation("Quotes");

                    b.Navigation("Settings")
                        .IsRequired();
                });

            modelBuilder.Entity("MADS.Entities.UserDbEntity", b =>
                {
                    b.Navigation("Incidents");

                    b.Navigation("Reminders");

                    b.Navigation("VoiceAlerts");
                });
#pragma warning restore 612, 618
        }
    }
}
