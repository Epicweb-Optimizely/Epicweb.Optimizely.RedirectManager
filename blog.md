# Redirect Manager 6.4: Supercharge Your URL Management with Import, Export, and Search

We're excited to announce version 6.4 of the Redirect Manager for Optimizely CMS, bringing three powerful new features that make managing your redirect rules easier and more efficient than ever before. Whether you're migrating between environments, performing bulk updates, or simply trying to find that one redirect rule among hundreds, these new features have you covered.

## Search Functionality: Find What You Need, Fast

Managing hundreds or even thousands of redirect rules can be overwhelming. The new **Search** feature lets you quickly filter through your rules in real-time, searching across all the important fields:

- **From URL**: Find rules by source address
- **To URL**: Search by destination
- **Host**: Filter by domain
- **Content ID**: Look up rules pointing to specific content

### How It Works

Simply type in the search box and watch as the table instantly filters to show only matching rules. The search is:
- **Real-time**: Results update as you type
- **Case-insensitive**: No need to worry about capitalization
- **Multi-field**: Searches across all relevant columns simultaneously
- **Smart**: Shows you how many rules match your search out of the total

```html
<!-- Example: Search for all rules related to "/products" -->
Type: /products
Result: "Showing 15 of 342 rules"
```

Perfect for:
- Finding specific redirects during troubleshooting
- Auditing rules for a particular section of your site
- Quickly locating rules that need updating

## Export Rules: Backup and Document with Ease

Need to document your redirects, share them with your team, or back them up before making changes? The new **Export** feature creates beautifully formatted Excel spreadsheets with all your redirect rules.

### Excel Export (.xlsx)
Creates a professional, formatted spreadsheet with all your redirect rules. Perfect for:
- Documentation and reporting
- Sharing with non-technical stakeholders
- Creating visual representations of your redirect strategy
- Easy viewing and editing in Excel, Google Sheets, or other spreadsheet applications
- Backing up your redirect configuration

### The "Convert to URL" Option

The export feature supports an optional **Convert to URL** checkbox. When enabled:
- Content IDs are automatically resolved to their corresponding URLs
- The `ToContentId` field is set to 0 in the export
- You get human-readable URLs instead of numeric IDs

This is incredibly useful when you want to:
- Review where your redirects actually point
- Share rules with people who don't have access to your CMS
- Document your redirect strategy in a more readable format

```plaintext
Example Export (with Convert to URL):
Order | Host        | From Url      | To Url                | To Content Id
1     | example.com | /old-product  | /products/new-widget  | 0
```

## Import Rules: Bulk Operations Made Simple

The companion to Export, the new **Import** feature enables powerful bulk operations on your redirect rules. Upload Excel (.xlsx) files to add or update multiple rules at once.

### Two Import Modes

#### 1. Update Mode (Default - Safe)
This is the recommended mode for most scenarios:
- Matches existing rules by "From URL" and "Host"
- Updates rules if a match is found
- Adds new rules if no match exists
- Preserves rules not in the import file

**Use cases:**
- Updating redirect destinations in bulk
- Adding new rules from a prepared file
- Syncing changes between environments

#### 2. Replace All Mode (Use with Caution)
For complete rule replacement:
- Deletes ALL existing redirect rules first
- Then imports all rules from the file
- Perfect for restoring backups or starting fresh

**Use cases:**
- Restoring a complete backup
- Migrating from another redirect system
- Complete environment setup

### File Format Requirements

The import expects a file with these columns:
- **Order**: Numeric priority (lower numbers processed first)
- **Host**: Domain name or "*" for all domains
- **From Url**: Source URL (must start with "/")
- **Wildcard**: "Yes" or "No"
- **To Url**: Destination URL
- **To Content Id**: Numeric ID or 0
- **Language**: Language code or empty

### Error Handling

One of the best features of the import functionality is its robust error handling:
- **Detailed Error Messages**: See exactly which rows have problems
- **Partial Success**: Successfully imports valid rows even if some fail
- **Clear Feedback**: Shows counts of added, updated, and deleted rules

```plaintext
Example Import Result:
? Import completed successfully
  45 rules added
  12 rules updated
  2 errors encountered

Errors:
  Line 15: Invalid format - missing From Url
  Line 23: ToUrl and ToContentId both empty
```

## Real-World Scenarios

### Scenario 1: Environment Migration
You're moving from staging to production:
1. **Export** all rules from staging as Excel
2. Review the exported file
3. **Import** into production using Update Mode
4. **Search** to verify specific rules migrated correctly

### Scenario 2: Bulk URL Updates
Your product URLs changed from `/products/` to `/shop/`:
1. **Export** as Excel
2. Open in Excel or Google Sheets and update the URLs
3. Save and **Import** back using Update Mode
4. **Search** for "shop" to confirm the changes

### Scenario 3: Cleanup and Optimization
Spring cleaning your redirects:
1. **Export** for backup
2. Review in Excel, mark rules to keep
3. Delete unnecessary rows
4. **Import** with Replace All Mode
5. Use the **Cleanup** feature to remove duplicates

### Scenario 4: Documentation
Creating documentation for your redirects:
1. **Export** as Excel with "Convert to URL" enabled
2. Get a readable spreadsheet with actual URLs
3. Share with marketing/SEO team
4. They can request changes without needing CMS access

## Pro Tips

1. **Always Export Before Major Changes**: Create a backup export before using Replace All Mode or making bulk updates.

2. **Use Update Mode First**: When unsure, start with Update Mode. It's safer and you can always export, modify, and re-import.

3. **Convert to URL for Reviews**: When exporting for review or documentation, enable "Convert to URL" for better readability.

4. **Combine Search with Export**: Use search to filter rules, then export only what you need (note: current version exports all rules, but search helps you identify what to focus on).

5. **Test in Non-Production First**: Try import/export workflows in development or staging environments before production.

## Technical Details

### Supported File Formats
- **Excel**: .xlsx (Office Open XML)

### Performance
- **Search**: Real-time filtering with no server roundtrips
- **Export**: Handles thousands of rules efficiently
- **Import**: Processes large files with progress feedback

### Security
- All operations respect ASP.NET antiforgery tokens
- Import validates file extensions and content
- Confirmation prompts for destructive operations

## Getting Started

The new features are immediately available in Redirect Manager 6.4. To use them:

1. Navigate to **CMS** ? **Admin** ? **Redirect Manager**
2. Look for the new accordion panels:
   - **Search Rules**: For finding specific redirects
   - **Export Rules**: For backing up or sharing rules
   - **Import Rules**: For bulk operations

Each panel includes built-in instructions and help text to guide you through the process.

## What's Next?

We're committed to making Redirect Manager even better. Some ideas we're exploring:
- Scheduled exports for automatic backups
- Bulk delete
- Delete in search mode
- Import preview mode
- Advanced search with regex support

Have suggestions? We'd love to hear them! Visit our [GitHub repository](https://github.com/epicweb-optimizely/epicweb.optimizely.redirectmanager) to share feedback or report issues.

## Acknowledgments

These features were designed with real-world usage in mind, based on feedback from the Optimizely community. Thank you to everyone who has contributed ideas, bug reports, and feature requests!

---

## Summary

Redirect Manager 6.4 introduces three game-changing features:

| Feature | Purpose | Key Benefit |
|---------|---------|-------------|
| **Search** | Find specific rules quickly | Save time when managing many rules |
| **Export** | Backup and document rules | Professional Excel format for easy sharing |
| **Import** | Bulk add or update rules | Efficient environment migration and bulk updates |

Whether you're managing a dozen redirects or thousands, version 6.4 makes your life easier. Update today and experience the difference!

---

**Download**: Available on [NuGet](https://www.nuget.org/packages/Epicweb.Optimizely.RedirectManager)  
**Source Code**: [GitHub](https://github.com/epicweb-optimizely/epicweb.optimizely.redirectmanager)  
**License**: Apache 2.0

Happy redirecting!
