# About Cubusky S3
Cubusky S3 is a package for integrating S3 Saving & Loading inside your project.

#### ⚠️ WARNING ⚠️
Cubusky S3 does not encrypt your credentials and provides no security features! This is something you will need to integrate yourself. Be aware that storing access keys inside your public Unity project can be used by hackers to, in the worst case, mine bitcoins on your S3 server, accruing massive debt for you to pay off.

Only use this package if you know what you're doing!

## Installing Cubusky S3
To install this package, follow the instructions on the [Package Manager documentation](https://docs.unity3d.com/Manual/upm-ui-giturl.html) after adding the following lines in your `manifest.json`
```json
{
  "dependencies": {
    "com.cubusky.core": "https://github.com/Cubusky/com.cubusky.core.git#1.2"
  }
}
```

## Requirements
This version of _Templates Package_ is compatible with the following versions of the Unity Editor:
- 2022.3 and later (recommended)

## Known Limitations
Cubusky S3 `1.0.0` includes the following known limitations:
- No security features have been implemented.
