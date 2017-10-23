# ScrapeQL
Query language for webscraping
## Dependencies
We are using the packages "HTMLAgilityPack" and "Csharpmonad" to realize this project.

## Syntax
Regarding the syntax, ScrapeQL is much alike its famous paragon, SQL.

There are several queries.

1. Load-Query: Loads a website or HTML File for further selection into a virtual workspace.
```
LOAD "filename.fileextension/url" AS Identifier
```

2. Write-Query: Writes the finished selection into filename.filextension.
```
WRITE identifier TO "filename.fileextension"
```


3. Select-Query: Selfexplanatory... Selects from identifier using given selectors.
```
SELECT "selector" FROM identifier <WHERE attribute=value>
```

