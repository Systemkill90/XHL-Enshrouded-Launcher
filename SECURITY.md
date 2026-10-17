# Security policy

XHL is experimental and **is not a hardened remote-admin solution**. There is no production-support or guaranteed patch SLA.

## Reporting
For suspected vulnerabilities, **do not publish exploit details, credentials, tokens, identifiable player logs, or save data in public issues**. Contact the repository owner privately through a channel you already trust. A private vulnerability-reporting channel has not been configured for this project.

## Sensitive files not to publish
Keep private server configs, saves, player data, local paths, keys, Steam credentials, runtime logs, dump files, and third-party/modified game binaries out of this public repository. Audit every import for secrets and distribution rights.

## High-risk operations
Update, restore, file migration and mod installation must offer backups, path validation, confirmations, rollback, and never silently manipulate a live server. Test on disposable copies first.
