using System.Globalization;
using System.Collections.Generic;

namespace FunnyActivities.Application.Validators;

public static class ValidationMessageProvider
{
    private static readonly Dictionary<string, (string tr, string en)> Messages = new()
    {
        // Common auth/user
        ["EmailRequired"] = ("Email zorunludur.", "Email is required."),
        ["InvalidEmailFormat"] = ("Gecersiz email formati.", "Invalid email format."),
        ["EmailDomainInvalid"] = ("Email gecerli bir alan adindan olmalidir.", "Email must be from a valid domain."),
        ["PasswordRequired"] = ("Sifre zorunludur.", "Password is required."),
        ["PasswordMinLength"] = ("Sifre en az 8 karakter olmalidir.", "Password must be at least 8 characters long."),
        ["PasswordUppercase"] = ("Sifre en az bir buyuk harf icermelidir.", "Password must contain at least one uppercase letter."),
        ["PasswordLowercase"] = ("Sifre en az bir kucuk harf icermelidir.", "Password must contain at least one lowercase letter."),
        ["PasswordNumber"] = ("Sifre en az bir rakam icermelidir.", "Password must contain at least one number."),
        ["PasswordSpecial"] = ("Sifre en az bir ozel karakter icermelidir.", "Password must contain at least one special character."),
        ["FirstNameRequired"] = ("Ad zorunludur.", "First name is required."),
        ["FirstNameMax"] = ("Ad 50 karakteri gecemez.", "First name must not exceed 50 characters."),
        ["LastNameRequired"] = ("Soyad zorunludur.", "Last name is required."),
        ["LastNameMax"] = ("Soyad 50 karakteri gecemez.", "Last name must not exceed 50 characters."),
        ["TokenRequired"] = ("Token zorunludur.", "Token is required."),
        ["NewPasswordRequired"] = ("Yeni sifre zorunludur.", "New password is required."),

        // Common entity fields
        ["NameRequired"] = ("Isim zorunludur.", "Name is required."),
        ["NameMax100"] = ("Isim 100 karakteri asamaz.", "Name cannot exceed 100 characters."),
        ["NameLength1To100"] = ("Isim 1 ile 100 karakter arasinda olmalidir.", "Name must be between 1 and 100 characters."),
        ["DescriptionMax500"] = ("Aciklama 500 karakteri asamaz.", "Description cannot exceed 500 characters."),
        ["UserIdRequired"] = ("Kullanici ID zorunludur.", "User ID is required."),
        ["CategoryIdRequired"] = ("Kategori ID zorunludur.", "Category ID is required."),
        ["CategoryIdInvalid"] = ("Kategori ID gecerli bir GUID olmalidir.", "Category ID must be a valid GUID."),
        ["PageSizeMax100"] = ("Sayfa boyutu 100'u gecemez.", "Page size cannot exceed 100."),
        ["SearchTermRequired"] = ("Arama terimi zorunludur.", "Search term is required."),

        // Product/Variant specific
        ["VariantIdRequired"] = ("Varyant ID zorunludur.", "Variant ID is required."),
        ["BaseProductIdRequired"] = ("Temel urun ID zorunludur.", "Base product ID is required."),

        // Survey specific
        ["SurveyTitleRequired"] = ("Anket basligi zorunludur.", "Survey title is required."),
        ["SurveyTitleMax200"] = ("Anket basligi 200 karakteri asamaz.", "Survey title cannot exceed 200 characters."),
        ["SurveyTitleMin3"] = ("Anket basligi en az 3 karakter olmalidir.", "Survey title must be at least 3 characters long."),
        ["SurveyDescriptionMax1000"] = ("Anket aciklamasi 1000 karakteri asamaz.", "Survey description cannot exceed 1000 characters."),
        ["SurveyStartDateRequired"] = ("Anket baslangic tarihi zorunludur.", "Survey start date is required."),
        ["SurveyStartDateValid"] = ("Anket baslangic tarihi gecerli olmalidir.", "Survey start date must be a valid date."),
        ["SurveyStartDateNotPast"] = ("Anket baslangic tarihi gecmiste olamaz.", "Survey start date cannot be in the past."),
        ["SurveyEndDateAfterStart"] = ("Anket bitis tarihi baslangictan sonra olmalidir.", "Survey end date must be after start date."),
        ["SurveyMaxParticipantsPositive"] = ("Maksimum katilimci 0'dan buyuk olmalidir.", "Maximum participants must be greater than 0."),
        ["SurveyActivityRequired"] = ("En az bir aktivite zorunludur.", "At least one activity is required."),
        ["SurveyActivityIdInvalid"] = ("Aktivite ID'leri bos olamaz.", "Activity IDs cannot be empty."),
        ["SurveyActivityIdRequired"] = ("Anket aktivite ID'si zorunludur.", "Survey activity ID is required."),
        ["RequestInvalid"] = ("Istek gecerli degil.", "Request contains invalid data."),
        ["SurveyUpdateRequestInvalid"] = ("Guncelleme icin en az bir alan saglanmalidir.", "Request contains invalid data - at least one field must be provided for update."),
        ["VoteValueRequired"] = ("Oy degeri zorunludur.", "Vote value is required."),
        ["VoteValueRange"] = ("Oy degeri 1 ile 5 arasinda olmalidir.", "Vote value must be between 1 and 5."),

        // Activity / Step
        ["ActivityIdRequired"] = ("Aktivite ID zorunludur.", "Activity ID is required."),
        ["ActivityNameRequired"] = ("Aktivite adi zorunludur.", "Activity name is required."),
        ["ActivityNameLength1To200"] = ("Aktivite adi 1-200 karakter arasinda olmalidir.", "Activity name must be between 1 and 200 characters."),
        ["ActivityDescriptionMax1000"] = ("Aktivite aciklamasi 1000 karakteri asamaz.", "Activity description cannot exceed 1000 characters."),
        ["ActivityVideoUrlInvalid"] = ("Gecerli bir video URL'si girin.", "Invalid video URL format."),
        ["ActivityIntroVideoUrlInvalid"] = ("Gecerli bir intro video URL'si girin.", "Invalid intro video URL format."),
        ["ActivityCategoryIdRequired"] = ("Aktivite kategorisi zorunludur.", "Activity category is required."),
        ["DurationHoursRange"] = ("Saat 0-23 arasinda olmalidir.", "Hours must be between 0 and 23."),
        ["DurationMinutesRange"] = ("Dakika 0-59 arasinda olmalidir.", "Minutes must be between 0 and 59."),
        ["DurationSecondsRange"] = ("Saniye 0-59 arasinda olmalidir.", "Seconds must be between 0 and 59."),
        ["StepOrderGreaterThan0"] = ("Sira 0'dan buyuk olmalidir.", "Order must be greater than 0."),
        ["StepDescriptionRequired"] = ("Aciklama zorunludur.", "Description is required."),
        ["StepDescriptionLength1To1000"] = ("Aciklama 1-1000 karakter arasinda olmalidir.", "Description must be between 1 and 1000 characters."),
        ["TimestampSecondsNonNegative"] = ("Zaman damgasi negatif olamaz.", "Timestamp seconds must be non-negative."),

        // Category
        ["CategoryNameRequired"] = ("Kategori adi zorunludur.", "Category name is required."),
        ["CategoryNameLength1To100"] = ("Kategori adi 1-100 karakter arasinda olmalidir.", "Category name must be between 1 and 100 characters."),

        // Pagination / sorting / search
        ["SearchTermMax100"] = ("Arama terimi 100 karakteri asamaz.", "Search term cannot exceed 100 characters."),
        ["SearchTermMin1"] = ("Arama terimi en az 1 karakter olmalidir.", "Search term must be at least 1 character long."),
        ["PageNumberMin1"] = ("Sayfa numarasi 0'dan buyuk olmalidir.", "Page number must be greater than 0."),
        ["PageSizeMin1"] = ("Sayfa boyutu 0'dan buyuk olmalidir.", "Page size must be greater than 0."),
        ["SortByInvalid"] = ("Gecersiz siralama alani.", "Invalid sort field."),
        ["SortOrderInvalid"] = ("Gecersiz siralama yonu.", "Invalid sort order."),

        // Product variant
        ["ProductVariantIdRequired"] = ("Urun varyant ID zorunludur.", "Product variant ID is required."),
        ["StockQuantityNonNegative"] = ("Stok miktari negatif olamaz.", "Stock quantity cannot be negative."),
        ["UnitOfMeasureIdRequired"] = ("Olcu birimi ID zorunludur.", "Unit of measure ID is required."),
        ["UnitValuePositive"] = ("Birim degeri 0'dan buyuk olmalidir.", "Unit value must be greater than zero."),
        ["UsageNotesMax300"] = ("Kullanim notlari 300 karakteri asamaz.", "Usage notes cannot exceed 300 characters."),

        // Persona
        ["PersonaNameRequired"] = ("Persona adi zorunludur.", "Persona name is required."),
        ["PersonaNameMax100"] = ("Persona adi 100 karakteri asamaz.", "Persona name must not exceed 100 characters."),
        ["PersonaNameInvalidCharacters"] = ("Persona adi gecersiz karakterler iceriyor.", "Persona name contains invalid characters."),
        ["PersonaDescriptionMax500"] = ("Aciklama 500 karakteri asamaz.", "Description must not exceed 500 characters."),
        ["PersonaAgePositive"] = ("Yas pozitif olmalidir.", "Age must be a positive integer."),
        ["PersonaGenderInvalid"] = ("Gecersiz cinsiyet degeri.", "Invalid gender value."),
        ["PersonaNationalityMax100"] = ("Uyruk 100 karakteri asamaz.", "Nationality must not exceed 100 characters."),
        ["PersonaBiographyMax2000"] = ("Biyografi 2000 karakteri asamaz.", "Biography must not exceed 2000 characters."),
        ["PersonaAvatarUrlInvalid"] = ("Gecerli bir avatar URL'si girin.", "Avatar image URL must be a valid URL."),
        ["PersonaCharacteristicNameRequired"] = ("Ozellik adi zorunludur.", "Characteristic name is required."),
        ["PersonaCharacteristicNameMax50"] = ("Ozellik adi 50 karakteri asamaz.", "Characteristic name must not exceed 50 characters."),
        ["PersonaCharacteristicValueRequired"] = ("Ozellik degeri zorunludur.", "Characteristic value is required."),
        ["PersonaCharacteristicValueMax200"] = ("Ozellik degeri 200 karakteri asamaz.", "Characteristic value must not exceed 200 characters."),
        ["PersonaCharacteristicOrderNonNegative"] = ("Ozellik sirasi negatif olamaz.", "Order must be non-negative."),

        // Content generation
        ["PersonaIdRequired"] = ("Persona ID zorunludur.", "Persona ID is required."),
        ["CustomPromptMax1000"] = ("Ozel istem 1000 karakteri asamaz.", "Custom prompt must not exceed 1000 characters."),
        ["ModelRequired"] = ("Model zorunludur.", "Model is required."),
        ["ModelInvalid"] = ("Gecersiz model belirtildi.", "Invalid model specified."),
        ["ProviderInvalid"] = ("Gecersiz LLM saglayicisi secildi.", "Invalid LLM provider selected."),
        ["TemperatureRange"] = ("Sicaklik 0 ile 2 arasinda olmalidir.", "Temperature must be between 0 and 2."),
        ["MaxTokensRange"] = ("Maksimum token degeri 0'dan buyuk olmalidir.", "Max tokens must be greater than zero."),
    };

    public static string Get(string key)
    {
        if (!Messages.TryGetValue(key, out var value))
        {
            return key;
        }

        var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        return culture == "tr" ? value.tr : value.en;
    }
}
