# Using NicknameGenerator in Unity

`NicknameGenerator` targets `netstandard2.0` and has **zero dependencies** beyond
the BCL, so it works in Unity 2019.4+ with Mono or IL2CPP.

## Setup

1. **Build the library** as a `netstandard2.0` DLL:

   ```bash
   dotnet build src/NicknameGenerator/NicknameGenerator.csproj -c Release -f netstandard2.0
    ```
    Output: `src/NicknameGenerator/bin/Release/netstandard2.0/NicknameGenerator.dll`
2. **Copy the DLL** into your Unity project:
    ```text
    Assets/Plugins/NicknameGenerator.dll
    ```
3. Select the DLL in Unity -> Inspector -> Plugin settings:
    - **Platforms:** Any Platform
    - **Auto Reference:** ✓
4. **IL2CPP build:** if you enable **Managed Stripping Level:** Medium or higher, add `link.xml` to `Assets/` to preserve the types (see below).

## Minimal usage

See `Example.cs`. Attach it to any `GameObject`.

⚠️ **Do not hardcode the salt in a client-side build** — anyone with a decompiler can extract it. Get it from your backend at runtime and cache it in memory.

## IL2CPP `link.xml` (optional but recommended)

If you use Managed Stripping `Medium` or `High`:

```xml
<linker>
  <assembly fullname="NicknameGenerator" preserve="all" />
</linker>
```

## Tested configurations

- ✅ Unity 2022.3 LTS, Mono, standalone macOS build.
- ✅ Unity 2022.3 LTS, IL2CPP, standalone macOS build, Managed Scripting = Medium.