EnsureDataLoaded();

UndertaleModLib.Compiler.CodeImportGroup importGroup = new(Data)
{
    ThrowOnNoOpFindReplace = true,
    MainThreadAction = MainThreadAction
};

// Находим Create Event игрока
if (Data.Code.ByName("gml_Object_obj_kris_Create_0") is not UndertaleCode playerCreateCode)
{
    ScriptError("Failed to find gml_Object_obj_kris_Create_0.");
    return;
}

// Находим Step Event игрока
if (Data.Code.ByName("gml_Object_obj_kris_Step_0") is not UndertaleCode playerStepCode)
{
    ScriptError("Failed to find gml_Object_obj_kris_Step_0.");
    return;
}

// Находим Draw Event игрока
if (Data.Code.ByName("gml_Object_obj_kris_Draw_0") is not UndertaleCode playerDrawCode)
{
    ScriptError("Failed to find player draw event.");
    return;
}

// КОД ДЛЯ CREATE EVENT
string createNetworkCode = @"
// === MULTIPLAYER INIT ===
global.sock = network_create_socket(network_socket_ws);
network_connect(global.sock, ""127.0.0.1"", 8080);
global.player_id = -1;
global.players = ds_map_create();
// === END MULTIPLAYER INIT ===
";

importGroup.QueueFindReplace(
    playerCreateCode,
    "image_xscale = 1;",
    createNetworkCode + "\nimage_xscale = 1;"
);

// КОД ДЛЯ STEP EVENT - ПРОСТАЯ ВЕРСИЯ БЕЗ СЛОЖНОГО JSON
string stepNetworkCode = @"
// === MULTIPLAYER UPDATE ===
if (global.sock != -1) {
    var buff = buffer_create(1024, buffer_fixed, 1);
    var msg = ""move|"" + string(x) + ""|"" + string(y);
    buffer_write(buff, buffer_string, msg);
    network_send_raw(global.sock, buff, buffer_get_size(buff), network_send_text);
    buffer_delete(buff);
}
// === END MULTIPLAYER UPDATE ===
";

importGroup.QueueFindReplace(
    playerStepCode,
    "if (global.CurrentKrisState != global.KrisStates.Walking) {",
    stepNetworkCode + "\nif (global.CurrentKrisState != global.KrisStates.Walking) {"
);

// КОД ДЛЯ DRAW EVENT
string drawNetworkCode = @"
// === MULTIPLAYER DRAW ===
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
// === END MULTIPLAYER DRAW ===
";

importGroup.QueueFindReplace(
    playerDrawCode,
    "draw_self();",
    "draw_self();\n" + drawNetworkCode
);

importGroup.Import();
ChangeSelection(playerCreateCode);