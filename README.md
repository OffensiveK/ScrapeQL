# ScrapeQL
Query language for webscraping

Regarding the syntax, ScrapeQL is much alike its famous paragon, SQL.

There are several queries.
Load-Query
Loads a website or HTML File for further selection into a virtual workspace.
```
LOAD "filename.fileextension/url" AS Identifier
```

Write-Query
Writes the finished selection into filename.filextension.
```
WRITE identifier TO "filename.fileextension"
```


Select-Query
Selfexplanatory... Selects from identifier using given selectors.
```
SELECT "selector" FROM identifier <WHERE attribute=value>
```
