# Changelog

## 2.0.0 - 04-21-2024

- v2.0.0 renames this package from `functions-csharp` to `Supabase.Functions`. The depreciation notice has been set in NuGet. The API remains the same.
- Re: [#135](https://github.com/supabase-community/supabase-csharp/issues/135) Update nuget package
  name `functions-csharp` to `Supabase.Functions`

## 1.3.2 - 03-12-2024

- Re: [#5](https://github.com/supabase-community/functions-csharp/issues/5) Add support for specifying Http Timeout on a function call by adding `HttpTimeout` to `InvokeFunctionOptions`

## 1.3.1 - 06-10-2023

- Updates usage of `Supabase.Core` assembly.

## 1.3.0 - 06-10-2023

- Rename assembly to `Supabase.Functions`
- Uses `FunctionsException` instead of `RequestException`

## 1.2.1 - 11-12-2022

- Use `supabase-core` and implement `IGettableHeaders` on `Client`

## 1.2.0 - 2022-11-10

- [MINOR] `Client` now initializes with a `baseUrl` and method calls arguments are only the `functionName`.
- Included `GetHeaders` property.

## 1.1.0 - 2022-11-04

- `Client` is no longer a Singleton class, it should be initialized using a default constructor.
- [#1](https://github.com/supabase-community/functions-csharp/issues/1) Restructures library to support DI.

## 1.0.1 - 2022-04-15

- Default `token` to be `null` in `Invoke` calls to allow `Authorization` to be passed solely via Headers.

## 1.0.0 - 2022-04-14

- Initial Release