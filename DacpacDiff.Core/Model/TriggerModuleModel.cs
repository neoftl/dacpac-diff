﻿using DacpacDiff.Core.Utility;

namespace DacpacDiff.Core.Model;

public class TriggerModuleModel(SchemaModel schema, string name)
    : ModuleWithBody(schema, name, ModuleType.TRIGGER)
{
    public string Parent { get; set; } = string.Empty;

    public string? ExecuteAs { get; set; }

    /// <summary>
    /// True if a BEFORE (INSTEAD OF) trugger, else AFTER
    /// </summary>
    public bool Before { get; set; }

    public bool ForDelete { get; set; }
    public bool ForInsert { get; set; }
    public bool ForUpdate { get; set; }

    public override bool IsSimilarDefinition(ModuleModel other)
    {
        if (other is not TriggerModuleModel trig)
        {
            return false;
        }

        return this.IsEqual(trig,
            m => m.ExecuteAs,
            m => m.Before,
            m => m.ForDelete,
            m => m.ForInsert,
            m => m.ForUpdate,
            m => m.Body.ScrubSQL());
    }
}
