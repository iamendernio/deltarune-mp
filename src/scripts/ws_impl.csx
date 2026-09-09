EnsureDataLoaded();

#region "TODO: find all needed code, rewrite some bullshit"
#endregion
UndertaleModLib.Compiler.CodeImportGroup importGroup = new(Data)
{
    ThrowOnNoOpFindReplace = true,
    MainThreadAction = MainThreadAction
};



if (Data.Code.ByName("gml_Object_obj_mainchara_Create_0") is not UndertaleCode playerCreateCode)
{
    ScriptError("Failed to find gml_Object_obj_mainchara_Create_0");
    return;
}


if (Data.Code.ByName("gml_Object_obj_mainchara_Step_0") is not UndertaleCode playerStepCode)
{
    ScriptError("Failed to find gml_Object_obj_mainchara_Step_0");
    return;
}


if (Data.Code.ByName("gml_Object_obj_mainchara_Draw_0") is not UndertaleCode playerDrawCode)
{
    ScriptError("Failed to find player draw event.");
    return;
}
string createNetworkCode = @"
#region ""create network code""
global.sock = network_create_socket(network_socket_ws);
network_connect(global.sock, ""127.0.0.1"", 8080);
global.player_id = -1;
global.players = ds_map_create();
#endregion
";

importGroup.QueueFindReplace(
    playerCreateCode,
    "image_xscale = 1;",
    createNetworkCode + "\nimage_xscale = 1;"
);

string stepNetworkCode = @"
#region ""stepNetworkCode""
if (global.sock != -1) {
    var buff = buffer_create(1024, buffer_fixed, 1);
    var msg = ""move|"" + string(x) + ""|"" + string(y);
    buffer_write(buff, buffer_string, msg);
    network_send_raw(global.sock, buff, buffer_get_size(buff), network_send_text);
    buffer_delete(buff);
}
#endregion
";

importGroup.QueueFindReplace(
    playerStepCode,
    "if (global.CurrentKrisState != global.KrisStates.Walking) {",
    stepNetworkCode + "\nif (global.CurrentKrisState != global.KrisStates.Walking) {"
);


string drawNetworkCode = @"
#region ""drawNetworkCode""
var players = global.players;
var keys = ds_map_keys(players);
for (var i = 0; i < ds_list_size(keys); i++) {
    var id = ds_list_find_value(keys, i);
    if (id != global.player_id) {
        var data = players[? id];
        if (data != undefined) {
            draw_sprite_ext(sprite_index, 0, data[? ""x""], data[? ""y""], 1, 1, 0, c_white, 1);
        }
    }
}
#endregion
";

importGroup.QueueFindReplace(
    playerDrawCode,
    "draw_self();",
    "draw_self();\n" + drawNetworkCode
);

// string asyncNetworkCode = @"
// #region ""asyncNetworkCode""
// var async_id = ds_map_find_value(async_load, ""id"");
// if (async_id == global.sock) {
//     var type = ds_map_find_value(async_load, ""type"");
//     if (type == ""text"") {
//         var json = ds_map_find_value(async_load, ""result"");
//         var data = json_decode(json);
//         var player_id = data[? ""id""];
//         var player_x = data[? ""x""];
//         var player_y = data[? ""y""];
//         if (global.player_id == -1) {
//             global.player_id = player_id;
//         }
//         if (player_id != global.player_id) {
//             var player_data = global.players[? player_id];
//             if (player_data == undefined) {
//                 global.players[? player_id] = ds_map_create();
//             }
//             global.players[? player_id][? ""x""] = player_x;
//             global.players[? player_id][? ""y""] = player_y;
//         }
//     }
// }
// #endregion
// ";


importGroup.Import();
ChangeSelection(playerCreateCode);