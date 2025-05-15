= Data Model<sec:data-model>
The data model contains pure data that is either encoded within the excel sheet in large quantities, or can be swapped for a connection with an external database. The connection with the external database is outside the scope of this thesis.

This IR models the inputs and static values used in computations and contains no logic of its own. Instead, it represents the contents of cells and tables, external inputs, constants, and mock or example data that may be used in testing. It exists to cleanly separate raw values from the formulas that act upon them.

The data model closely collaborates with the compute model, since the compute model can refer to data in the data model using the `DataReference` reference. 

The power of the data layer is in the way that it allows for more data than is originally in the excel sheet. It translates the dynamic of adding more data to the excel sheet in the static context of the compiler. Take the expenses for instance. One month, you might have the default 20 expenses. However, the next month you decide to throw a party, and need an extra trip to the supermarket, pay for a DJ, etc. The compiled spreadsheet should be able to handle this extra data, which is made possible by the repositories.

```cs
record DataRepository(Range Location);
record ExternalDataRepository(Range Location);
```
== Repository

The data of the excel sheet is stored in several _data repositories_. Data repositories can be seen as virtual regions in the excel sheet, where they represent certain data. For instance, in the budget example, multiple data repositories can be found: the categories, the expenses, the income. 

The expenses region is a table. A table is always mapped to a repository and will contain a _columnar schema_ (see next subsection). As discussed previously, the table may contain computed columns. These are not contained in the schema, since they are not data, but are computed _from_ the data. 

Besides the already discussed advantages of handling extra or different data than originally in the compiled spreadsheet, a repository is able to source it's data from one of multiple data sources. These data sources can be internal or external.

The repository always represents data from the structural layer and therefore by inference the excel sheet, which are both grid-like in nature. The data in the excel sheet and the structural layer is stored in a grid pattern. Hence, the repository will also store the data in a grid-like fashion. 

=== Internal Data
Internal data is data in the spreadsheet. In an extraction pass, the data in the spreadsheet is parsed into the repository. When parsing the data, the type is inferred or checked if it conforms to the already established schema. This internal data is then used in the computations. When using internal data, the result of the computation will match the result of Excel.


=== External Data
The more interesting option is the option for external data. The use of external data poses an interesting question on how to integrate with external systems. Does the compiler take these systems into account or not? 

If the compiler takes the systems into account, it will generate code that will connect to outside systems. For instance, it will generate code that connects with an _SQL Server_ Database in the _Microsoft Azure_ cloud, _DynamoDB_ in _Amazon Web Services_, or a _PostgreSQL_ database on premise. Apart from letting the user handling the authentication, this provides a user-friendly _plug and play_ experience where everything is handled. This also means that the generated project can be run directly from a CLI.

However, when the compiler takes the external systems into account, we have very tight coupling with these system. Furthermore, when new or possibly internal tools are introduced, it is up to the maintainer of the compiler to create new integrations and edit the compiler. This dependence is unwanted and unnecessary. Instead, the compiler will provide tooling which make integrating data easy.

If external tooling is required, the compiler will generate an input on the main computation that can be used to pass data. This means that conceptually, instead of creating a full program, the compiler creates a class library or big function that represents the spreadsheet. The input is based on the schema that is associated with the external repository. For instance, when we want to link an external database to the expenses table, the compiler emits an input with a type that is a list of expense items. It is up to the programmer to connect the database to emit this list, and pass it along to the generated program. In this way, when new databases or tooling is created, the compiler stays the same and the new tools can be integrated easily. 

== Schema
Just like the computations, the data has a type too. To represent this typing information separately from the data, we use a _schema_. This schema is a mapping from the names in the repository to their respective type. This mapping ensures consistent data typing and allows the compute and code layout model to query the type of a data reference. 

We distinguish different types of schemes. The _columnar schema_ is the most important schema. Here, the mapping does not consist of individual datapoints, but instead only allows for the mapping of columns to datatypes. This implies that all data in one column has to be the same. [Perhaps some other schema type (doesn't exist now)]

Finally, when there is no special data scheme to be recognized, we assign a _random schema_ where every datapoint in the repository can have a different type. While this schema is not as ordered as the rest and thus is smore difficult to work with, it allows for a fully type-safe schema.