using System;
using OrchardHarvest2025.Web.Plugins;

var plugin = new JsonDiffPlugin();
var original = "{\"title\":\"Teh Quick Fox\",\"tags\":[\"a\",\"b\"],\"meta\":{\"views\":10}}";
var updated = "{\"title\":\"The Quick Fox\",\"tags\":[\"a\",\"c\"],\"meta\":{\"views\":11}}";
var diff = plugin.JsonDiffAsync(original, updated).GetAwaiter().GetResult();
Console.WriteLine("Diff:\n" + diff);
// Basic assertions
var ok = diff.Contains("\"/title\"") && diff.Contains("\"/tags/1\"") && diff.Contains("\"/meta/views\"");
Environment.Exit(ok ? 0 : 1);

