using UnityEngine;
using NicknameGenerator;
using NicknameGenerator.Options;
using System;

/// <summary>
/// Minimal example: generate a deterministic nickname for the local player.
/// Attach to any GameObject in the scene.
/// </summary>
public class Example : MonoBehaviour
{
    [Tooltip("In production: load from your backend at runtime. NEVER commit real secrets.")]
    public string SecretSalt = "0123456789abcdef0123456789abcdef";

    private HmacNicknameGenerator _generator;
    private readonly Guid _userId = Guid.Parse("550e8400-e29b-41d4-a716-446655440000");

    private void Awake()
    {
        _generator = new(
                new NicknameOptions
                {
                    SecretSalt = SecretSalt
                }
            );
    }

    private void Start()
    {
        Debug.Log($"User {_userId}: {_generator.Generate(_userId)}"); // FrostyElk_00z5ReQiH
        Debug.Log($"Retry 1:        {_generator.Generate(_userId, 1)}"); // SpicyCrow_0j9XCwIfs
    }
}