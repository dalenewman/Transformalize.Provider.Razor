### Overview

This adds a `Razor` provider and transform to Transformalize using [RazorEngine](https://github.com/Antaris/RazorEngine).  It is a plug-in compatible with Transformalize 0.3.6-beta.

Build the Autofac project and put it's output into Transformalize's *plugins* folder.

### Write Usage

```xml
<add name='TestProcess' mode='init'>
  <connections>
    <add name='input' provider='bogus' seed='1' />
    <add name='output' provider='Razor' template='template.cshtml' file='output.html' />
  </connections>
  <entities>
    <add name='Contact' size='1000'>
      <fields>
        <add name='Identity' type='int' primary-key='true' />
        <add name='FirstName' />
        <add name='LastName' />
        <add name='Stars' type='byte' min='1' max='5' />
        <add name='Reviewers' type='int' min='0' max='500' />
      </fields>
    </add>
  </entities>
</add>
```

This writes 1000 rows of bogus data to *output.html*.

The template *template.cshtml* is passed a [RazorModel](https://github.com/dalenewman/Transformalize.Provider.Razor/blob/master/src/Transformalize.Provider.Razor/RazorModel.cs).  The template in this example looks like this:

```html
@model Transformalize.Providers.Razor.RazorModel
<!DOCTYPE html>
<html lang="en" xmlns="http://www.w3.org/1999/xhtml">
<head>
    <meta charset="utf-8" />
    <title>Razor Output</title>
</head>
<body>
    <table>
        <thead>
            <tr>
                @foreach (var field in Model.Entity.Fields.Where(f => !f.System)) {
                    <th>@field.Label</th>
                }
            </tr>
        </thead>
        <tbody>
            @foreach (var row in Model.Rows) {
                <tr>
                    @foreach (var field in Model.Entity.Fields.Where(f => !f.System)) {
                        <td>@(row[field])</td>
                    }
                </tr>
                ++Model.Entity.Inserts;
            }
        </tbody>
    </table>
</body>
</html>
```

The table in *output.html* looks like this (clipped for brevity):

<table>
        <thead>
            <tr>
                    <th>Identity</th>
                    <th>FirstName</th>
                    <th>LastName</th>
                    <th>Stars</th>
                    <th>Reviewers</th>
            </tr>
        </thead>
        <tbody>
                <tr>
                        <td>1</td>
                        <td>Justin</td>
                        <td>Konopelski</td>
                        <td>3</td>
                        <td>153</td>
                </tr>
                <tr>
                        <td>2</td>
                        <td>Eula</td>
                        <td>Schinner</td>
                        <td>2</td>
                        <td>41</td>
                </tr>
                <tr>
                        <td>3</td>
                        <td>Tanya</td>
                        <td>Shanahan</td>
                        <td>4</td>
                        <td>412</td>
                </tr>
                <tr>
                        <td>4</td>
                        <td>Emilio</td>
                        <td>Hand</td>
                        <td>4</td>
                        <td>469</td>
                </tr>
                <tr>
                        <td>5</td>
                        <td>Rachel</td>
                        <td>Abshire</td>
                        <td>3</td>
                        <td>341</td>
                </tr>
    </tbody>
</table>

### Benchmark

*Note: Numbers get better with more records.*

``` ini

BenchmarkDotNet=v0.11.1, OS=Windows 10.0.16299.251 (1709/FallCreatorsUpdate/Redstone3)
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
Frequency=2742189 Hz, Resolution=364.6722 ns, Timer=TSC
  [Host]       : .NET Framework 4.7.1 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2633.0
  LegacyJitX64 : .NET Framework 4.7.1 (CLR 4.0.30319.42000), 64bit LegacyJIT/clrjit-v4.7.2633.0;compatjit-v4.7.2633.0

Job=LegacyJitX64  Jit=LegacyJit  Platform=X64  
Runtime=Clr  

```
|                    Method |    Mean |    Error |   StdDev | Scaled | ScaledSD |
|-------------------------- |--------:|---------:|---------:|-------:|---------:|
|         &#39;10000 test rows&#39; | 1.100 s | 0.0230 s | 0.0358 s |   1.00 |     0.00 |
| &#39;10000 rows with 1 razor&#39; | 1.656 s | 0.0330 s | 0.0417 s |   1.51 |     0.06 |
