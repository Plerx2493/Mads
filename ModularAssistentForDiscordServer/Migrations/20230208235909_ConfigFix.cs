// Copyright 2023 Plerx2493
//
// Licensed under the Apache License, Version 2.0 (the "License")
// You may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MADS.Migrations
{
    /// <inheritdoc />
    public partial class ConfigFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Guilds_GuildConfigDbEntity_id",
                table: "Guilds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GuildConfigDbEntity",
                table: "GuildConfigDbEntity");

            migrationBuilder.RenameTable(
                name: "GuildConfigDbEntity",
                newName: "Configs");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Configs",
                table: "Configs",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Guilds_Configs_id",
                table: "Guilds",
                column: "id",
                principalTable: "Configs",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Guilds_Configs_id",
                table: "Guilds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Configs",
                table: "Configs");

            migrationBuilder.RenameTable(
                name: "Configs",
                newName: "GuildConfigDbEntity");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GuildConfigDbEntity",
                table: "GuildConfigDbEntity",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Guilds_GuildConfigDbEntity_id",
                table: "Guilds",
                column: "id",
                principalTable: "GuildConfigDbEntity",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
