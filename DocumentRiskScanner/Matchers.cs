using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DocumentRiskScanner
{
    // Lets us define a range of certainty of a match beyond just true/false,
    // might be useful for metrics or filtering
    // Of course, depending on the matcher, we may still only have true/false outcomes.
    // (That's fine, we just don't use intermediate values)
    public enum Match
    {
        Positive,
        Likely,
        Ambiguous,
        Unlikely,
        Negative
    }

    public static class Matchers
    {
        // Compares against a series of keywords that may suggest sensitive information is being stored here.
        // Keywords are defined in an input file and loaded into KeywordSearch
        public static Match IsKeyword(string candidate, KeywordSearch keywordSearch)
        {
            Match match = Match.Negative;
            var result = keywordSearch.Contains(candidate);
            if (result.success)
            {
                switch (result.riskLevel)
                {
                    case 0:
                        match = Match.Ambiguous;
                        break;
                    case 1:
                        match = Match.Likely;
                        break;
                    case 2:
                        match = Match.Positive;
                        break;
                    default:
                        break;
                }
            }

            return match;
        }

        // Looks for patterns that suggest an email address is being stored here.
        // Email address formatting guidelines are a nightmare so we're not checking for 'valid' emails here;
        // just strings that satisfy typical email address qualities
        public static Match IsEmail(string candidate)
        {
            var looseEmail = new Regex(@"^[^@]+@[^@]+$");
            var tighterEmail = new Regex(@"^[\w!#$%&'*+-/=?^_`{|}~.(),:;<>\[\]]{1,64}@[\w-.]{1,255}$");

            Match match = Match.Negative;
            candidate = candidate.ToLower().Where(c => !char.IsWhiteSpace(c)).ToString();

            if (looseEmail.IsMatch(candidate))
            {
                match = Match.Ambiguous;
                if(tighterEmail.IsMatch(candidate))
                {
                    match = Match.Positive;
                }
            }

            return match;
        }

        // Looks for patterns that suggest a (person's) name,
        // and then looks for their presence in a set of common names.
        // Are we only gonna match names with Latin chars? I guess we are. Sorry.
        public static Match IsName(string candidate, NameSearch nameSearch)
        {
            var nameRegex = new Regex(@"^[A-Z]([a-z]+)(-\s[A-Z]([a-z]+))*$");

            Match match = Match.Negative;
            if (nameRegex.IsMatch(candidate))
            {
                match = Match.Ambiguous;

                // Try to match against our collection of common first names
                // not sure what the perf of this is gonna be like :/
                var firstName = candidate.Split(' ')[0];
                if (nameSearch.Contains(firstName))
                {
                    match = Match.Positive;
                }
            }
            return match;
        }

        // Looks for patterns that suggest a phone number.
        // We are more certain of a match if the number has an acceptable length,
        // and spacing indicative of an Aussie mobile/landline number.
        public static Match IsPhoneNo(string candidate)
        {
            var loosePhone = new Regex(@"^\+?[\d-\s]$");
            var tighterLandline = new Regex(@"^(0\d)?[\s-_]?\d{4}[\s-_]?\d{4}$");
            var tighterMobile = new Regex(@"^(04\d\d)[\s-_]?\d{3}[\s-_]?\d{3}$");

            Match match = Match.Negative;
            if (loosePhone.IsMatch(candidate))
            {
                match = Match.Unlikely;
                var digits = candidate.Where(char.IsDigit).ToString();
                if (digits.Length >= 8 && digits.Length <= 11)
                {
                    match = Match.Ambiguous;
                    if (tighterLandline.IsMatch(candidate) || tighterMobile.IsMatch(candidate))
                    {
                        match = Match.Positive;
                    }
                }
            }

            return match;
        }

        // I think it's unlikely we have unmasked credit card numbers lying around anywhere,
        // but this looks for patterns that suggest card numbers anyway.
        // We become more certain if they conform to the XXXX-XXXX-XXXX-XXXX,
        // and if they identify themselves as being Visa/Mastercard
        public static Match IsCardNo(string candidate)
        {
            var looseCard = new Regex(@"^[\d-_\s]{8,}$");
            var stricterCard = new Regex(@"^(\d{4}[\s-_]?){4}$");

            Match match = Match.Negative;

            var digits = candidate.Where(char.IsDigit).ToString();
            if (digits.Length >= 8 && digits.Length <= 19 && looseCard.IsMatch(candidate))
            {
                match = Match.Ambiguous;

                if (stricterCard.IsMatch(candidate))
                {
                    match = Match.Likely;

                    // Mastercard and Visa start with 5 and 4 respectively
                    if (digits[0] == '4' || digits[0] == '5')
                    {
                        match = Match.Positive;
                    }
                }
            }

            return match;
        }
    }
}
