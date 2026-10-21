namespace NicknameGenerator.DependencyInjection
{
	using Microsoft.Extensions.Configuration;

	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.DependencyInjection.Extensions;

	using Microsoft.Extensions.Options;
	using NicknameGenerator.Abstractions;
	using NicknameGenerator.Options;


	/// <summary>
	/// Extension methods for registering <see cref="INicknameGenerator"/> and
	/// <see cref="IRandomNicknameGenerator"/> with dependency injection.
	/// </summary>
	public static class ServiceCollectionExtensions
	{
		private const string DEFAULT_SECTION_NAME = "Nickname";

		public static IServiceCollection AddNicknameGenerator(
			this IServiceCollection services,
			IConfiguration configuration,
			string sectionName = DEFAULT_SECTION_NAME
		)
		{
			if (services is null) throw new ArgumentNullException(nameof(services));
			if (configuration is null) throw new ArgumentNullException(nameof(configuration));

			services.AddOptions<NicknameOptions>()
				.Bind(configuration.GetSection(sectionName))
				.Validate(options => TryValidate(options, requireSecret: true, out _), "NicknameOptions is invalid. See NicknameOptions.Validate for details.")
				.ValidateOnStart();

			return services.AddHmacGenerator();
		}

		public static IServiceCollection AddNicknameGenerator(
			this IServiceCollection services,
			Action<NicknameOptions> configure
		)
		{
			if (services is null) throw new ArgumentNullException(nameof(services));
			if (configure is null) throw new ArgumentNullException(nameof(configure));

			services.AddOptions<NicknameOptions>()
				.Configure(configure)
				.Validate(options => TryValidate(options, requireSecret: true, out _), "NicknameOptions is invalid. See NicknameOptions.Validate for details.")
				.ValidateOnStart();

			return services.AddHmacGenerator();
		}

		public static IServiceCollection AddRandomNicknameGenerator(
			this IServiceCollection services,
			Action<NicknameOptions>? configure = null
		)
		{
			if (services is null) throw new ArgumentNullException(nameof(services));

			OptionsBuilder<NicknameOptions> _optionsBuilder = services.AddOptions<NicknameOptions>();
			if (configure is not null)
			{
				_optionsBuilder.Configure(configure);
			}
			_optionsBuilder
				.Validate(options => TryValidate(options, requireSecret: false, out _), "NicknameOptions is invalid for random generator.")
				.ValidateOnStart();

			services.TryAddSingleton<IRandomNicknameGenerator>(sp =>
			{
				NicknameOptions _options = sp.GetRequiredService<IOptions<NicknameOptions>>().Value;

				IWordListProvider? _wordListProvider = sp.GetService<IWordListProvider>();
				return new RandomNicknameGenerator(_options, _wordListProvider);
			});
			return services;
		}

		private static IServiceCollection AddHmacGenerator(this IServiceCollection services)
		{
			if (services.Any(sd => sd.ServiceType == typeof(IRetryableNicknameGenerator)))
			{
				services.TryAddSingleton<INicknameGenerator>(sp => sp.GetRequiredService<IRetryableNicknameGenerator>());
				return services;
			}
			
			services.TryAddSingleton(sp =>
			{
				NicknameOptions _options = sp.GetRequiredService<IOptions<NicknameOptions>>().Value;
				IWordListProvider? _provider = sp.GetService<IWordListProvider>();
				return new HmacNicknameGenerator(_options, _provider);
			});

			services.TryAddSingleton<IRetryableNicknameGenerator>(sp => sp.GetRequiredService<HmacNicknameGenerator>());
			services.TryAddSingleton<INicknameGenerator>(sp => sp.GetRequiredService<HmacNicknameGenerator>());

			return services;
		}

		private static bool TryValidate(NicknameOptions options, bool requireSecret, out string? errorMessage)
		{
			try
			{
				options.Validate(requireSecret);
				errorMessage = null;
				return true;
			}
			catch (ArgumentException _ex)
			{
				errorMessage = _ex.Message;
				return false;
			}
		}
	}
}
