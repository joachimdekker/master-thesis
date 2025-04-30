= Data Model<sec:data-model>
The data model contains pure data that is either encoded within the excel sheet in large quantities, or can be swapped for a connection with an external database. The connection with the external database is outside the scope of this thesis.

This IR models the inputs and static values used in computations and contains no logic of its own. Instead, it represents the contents of cells and tables, external inputs, constants, and mock or example data that may be used in testing. It exists to cleanly separate raw values from the formulas that act upon them.

The data model closely collaborates with the compute model, since the compute model can refer to data in the data model using the `DataReference` reference (which is different than the other references). 

```cs
record DataRepository(Range Location);
record ExternalDataRepository(Range Location);

record TableData(Range Location, List<string> Columns);
```

