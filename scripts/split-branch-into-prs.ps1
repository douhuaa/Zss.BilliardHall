#!/usr/bin/env pwsh

<#
Splits commits on the current branch into categorized PRs.
Categories (file globs -> label):
 - docs/** -> docs
 - scripts/** -> automation
 - .github/** -> ci
 - db/** -> db
 - *.yml, *.yaml -> ci
 - src/** -> code
 - others -> misc

Usage:
  pwsh .\split-branch-into-prs.ps1 -BaseBranch main -Remote origin -DryRun

Requirements:
 - git CLI available
 - gh CLI authenticated
#>

param(
    [string]$BaseBranch = 'main',
    [string]$Remote = 'origin',
    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'

function Get-CurrentBranch {
    git rev-parse --abbrev-ref HEAD
}

$current = (Get-CurrentBranch).Trim()
Write-Host "Current branch: $current"
if ($current -eq $BaseBranch) { Write-Host "Already on base branch $BaseBranch; abort."; exit 1 }

# Get list of commits that differ from base (oldest -> newest)
$commits = git rev-list --reverse $BaseBranch..$current
if (-not $commits) { Write-Host "No commits on branch $current relative to $BaseBranch"; exit 0 }

$categories = @{
    docs = 'docs/**'
    automation = 'scripts/**'
    ci = '.github/**'
    db = 'db/**'
    code = 'src/**'
}

function Categorize-Commit($commit) {
    $files = git diff-tree --no-commit-id --name-only -r $commit
    foreach ($f in $files) {
        foreach ($k in $categories.Keys) {
            $pattern = $categories[$k]
            # simple glob match
            if ($f -like $pattern) { return $k }
        }
        if ($f -match '\.ya?ml$') { return 'ci' }
    }
    return 'misc'
}

# Map category -> commit list
$catCommits = @{}
foreach ($c in $commits) {
    $cat = Categorize-Commit $c
    if (-not $catCommits.ContainsKey($cat)) { $catCommits[$cat] = @() }
    $catCommits[$cat] += $c
}

Write-Host "Categorized commits:"; $catCommits.Keys | ForEach-Object { Write-Host " - $_ ($($catCommits[$_].Count) commits)" }

foreach ($cat in $catCommits.Keys) {
    $branchName = "${current}-split-${cat}"
    Write-Host "Preparing branch $branchName for category $cat with $($catCommits[$cat].Count) commits"
    if ($DryRun) { Write-Host "DryRun: would create branch $branchName and cherry-pick commits"; continue }

    git checkout -b $branchName $BaseBranch
    foreach ($commit in $catCommits[$cat]) {
        git cherry-pick $commit
    }
    git push $Remote $branchName
    gh pr create --base $BaseBranch --head $branchName --title "[$cat] Split from $current" --body "Auto-split PR for category $cat from branch $current"
    git checkout $current
}

Write-Host "Done. Re-run with -DryRun:$false to execute."
