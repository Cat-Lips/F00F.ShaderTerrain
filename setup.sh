#!/bin/bash

# Remove this line to run without admin/developer mode and copy files instead of symlink
export MSYS=winsymlinks:nativestrict


#############################################################
### Run this script to download/clone external components ###
### Requires Admin or Developer Mode to create symlinks   ###
###  - Downloads & extracts addons from asset library     ###
###  - Clones & pulls from external git repos             ###
###  - Creates symlinks to addon/assets folder            ###
### By default, external addons are ignored               ###
###  - Commit addon/zip files to fix version              ###
#############################################################


addons=(
  #"1766 debug_draw_3d"
)

repos=(
  "https://github.com/Cat-Lips/F00F.git addons/F00F"
  "https://github.com/Cat-Lips/F00F.Core.git addons/F00F.Core"
  "https://github.com/Cat-Lips/F00F.ShaderNoise.git addons/F00F.ShaderNoise"
  #"https://github.com/Cat-Lips/F00F.ShaderTerrain.git addons/F00F.ShaderTerrain"

  "https://github.com/Cat-Lips/F00F.TerrainAssets.git Assets/Terrain"
)


##################
##### UTILS ######
##################

asset_api="https://godotengine.org/asset-library/api"

ext_repos=".."
ext_addons="../.godot/.addons"
ext_externals="../.godot/.externals"

init_paths() {
  if declare -p repos &>/dev/null && [ ${#repos[@]} -ne 0 ]; then mkdir -p "$ext_repos"; fi
  if declare -p addons &>/dev/null && [ ${#addons[@]} -ne 0 ]; then mkdir -p "$ext_addons"; fi
  if declare -p externals &>/dev/null && [ ${#externals[@]} -ne 0 ]; then mkdir -p "$ext_externals"; fi
}

RET() {
  echo "$@"
}

LOG() {
  echo "$@" >&2
  LAST_LOG="$@"
}

SEP() {
  if [ -n "$LAST_LOG" ]; then
    LOG
  fi
}

EXEC() {
  $@
  err=$?
  if [ $err -ne 0 ]; then
    LOG "*********************************"
    LOG "*** ERROR *** ERROR *** ERROR ***"
    LOG "*********************************"
    LOG "(failed with error code $err)"
    exit $err
  fi
}

pause() {
  if [[ $- == *i* ]]; then
    SEP; read -n1 -rsp "Press any key to continue..."
  fi
}


#############
### REPOS ###
#############

remote_master() {
  local git=$1

  RET $($git remote show origin | sed -n '/HEAD branch/s/.*: //p')
}

current_tag() {
  local git=$1

  RET $($git describe --tags --exact-match 2> /dev/null)
}

current_branch() {
  local git=$1

  RET $($git branch --show-current)
}

is_local_tag() {
  local git=$1
  local tag=$2

  if $git show-ref --verify --quiet refs/tags/$tag; then
    RET "X"
  fi
}

is_local_branch() {
  local git=$1
  local branch=$2

  if $git show-ref --verify --quiet refs/heads/$branch; then
    RET "X"
  fi
}

is_remote_tag() {
  local git=$1
  local tag=$2

  if $git ls-remote --tags origin | grep -q "$tag"; then
    RET "X"
  fi
}

is_remote_branch() {
  local git=$1
  local branch=$2

  if $git ls-remote --heads origin | grep -q "$branch"; then
    RET "X"
  fi
}

same_tag() {
  local git=$1
  local tag=$2

  if [ "$tag" == "$(current_tag "$git")" ]; then
    RET "X"
  fi
}

same_branch() {
  local git=$1
  local branch=$2

  if [ "$branch" == "$(current_branch "$git")" ]; then
    RET "X"
  fi
}

same_target() {
  local git=$1
  local target=$2

  if [ -n "$(same_tag "$git" "$target")" ] \
  || [ -n "$(same_branch "$git" "$target")" ]; then
    RET "X"
  fi
}

has_local_changes() {
  local git=$1

  if ! $git diff-index --quiet HEAD; then
    RET "X"
  fi
}

has_remote_changes() {
  local git=$1
  local branch=$2

  local local_commit=$($git rev-parse HEAD)
  local remote_commit=$($git ls-remote origin -h refs/heads/$branch | cut -f1)
  if [ "$local_commit" != "$remote_commit" ]; then
    RET "X"
  fi
}

git_target() {
  local git=$1
  local target=$2

  if [ -z "$target" ]; then
    target=$(remote_master "$git")
  fi

  RET "$target"
}

update_required() {
  local git=$1
  local target=$2;

  if [ -n "$(same_tag "$git" "$target")" ]; then
    return
  fi
 
  if [ -n "$(same_branch "$git" "$target")" ] \
  && [ -z "$(has_remote_changes "$git" "$target")" ]; then
    return
  fi

  RET "X"
}

git_checkout() {
  local log_id=$1
  local git=$2
  local target=$3
  local is_tag=$(is_remote_tag "$git" "$target")

  LOG "[$log_id] Switching to $target"

  if [ -n "$is_tag" ]; then
    local target_refs="refs/tags/$target:refs/tags/$target"
    local git_options=" -c advice.detachedHead=false"
  else
    local target_refs="$target:refs/heads/$target"
  fi

  local git_fetch="$git fetch origin $target_refs --depth 1"
  LOG "[$log_id]  - $git_fetch"
  EXEC $git_fetch

  local git_checkout="$git$git_options checkout $target"
  LOG "[$log_id]  - $git_checkout"
  EXEC $git_checkout
}

git_pull() {
  local log_id=$1
  local git=$2

  LOG "[$log_id] Pulling changes"

  local git_pull="$git pull --prune --rebase --depth 1"
  LOG "[$log_id]  - $git_pull"
  EXEC $git_pull
}

git_update() {
  local log_id=$1
  local git_dir=$2
  local target=$3
  local git="git -C $git_dir"
  target=$(git_target "$git" "$target")

  if [ -z "$(update_required "$git" "$target")" ]; then
    return
  fi

  if [ -n "$(has_local_changes "$git")" ]; then
    LOG "[$log_id] *** WARNING *** Local changes detected - Skipping update!"
    return
  fi

  if [ -z "$(same_target "$git" "$target")" ]; then
    git_checkout "$log_id" "$git" "$target"
  else
    git_pull "$log_id" "$git"
  fi

  #RET "X"
  return 1
}

git_clone() {
  local log_id=$1
  local git_url=$2
  local git_dir=$3
  local target=$4

  if [ -n "$target" ]; then
    local git_options=" -c advice.detachedHead=false"
    local clone_options=" -b $target"
  fi

  local git_clone="git$git_options clone$clone_options $git_url $git_dir --depth 1"
  LOG "[$log_id] Retrieving $git_url"
  LOG "[$log_id]  - $git_clone"

  EXEC $git_clone

  #RET "X"
  return 1
}

get_repo() {
  local ext_dir=$1
  local git_url=$2
  local local_dir=$3
  local branch_tag=$4
  local log_id="(git) $(basename $local_dir)"
  local git_dir="$ext_dir/$(basename ${git_url%.git})"

  if [ ! -d "$git_dir/.git" ]; then # if git repo missing
    git_clone "$log_id" "$git_url" "$git_dir" "$branch_tag"
    if [ $? -eq 1 ]; then local updated="X"; fi
  else
    git_update "$log_id" "$git_dir" "$branch_tag"
    if [ $? -eq 1 ]; then local updated="X"; fi
  fi

  if [ ! -d "$local_dir" ]; then # if symlink missing
    local symlink_source=$(find "$git_dir" -type d -path "*/$local_dir" -print -quit)
    if [ ! -d "$symlink_source" ]; then # if source missing
      if [ -n "$updated" ]; then
        LOG "[$log_id] *** '$addon_dir' not found - Enjoy content here: ***"
        LOG "[$log_id]  - $git_dir"
      fi
    else
      create_symlink="ln -rs $symlink_source $local_dir"
      LOG "[$log_id] Creating symlink"
      LOG "[$log_id]  - $create_symlink"
      mkdir -p "$(dirname $local_dir)"
      EXEC $create_symlink
    fi
  fi
}


##############
### ADDONS ###
##############

unescape() {
  echo "$1" | sed 's/\\//g'
}

parse_json() {
  local json=$1
  local token=$2
  local regex="\"$token\":\"[^\"]*\""
  local value=$(echo "$json" | grep -o $regex | cut -d'"' -f4)
  RET "$value"
}

get_addon() {
  local ext_dir=$1
  local asset_id=$2
  local addon_dir=$3
  local log_id="$asset_id"
  local addon_dir="addons/$addon_dir"
  local asset_url="$asset_api/asset/$asset_id"

  # Check for update
  local json=$(curl -sS "$asset_url")
  local title=$(parse_json "$json" title)
  local download_url=$(unescape $(parse_json "$json" download_url))
  local download_file="$ext_dir/$(basename $download_url)" # Remove path (longest */ from beginning)
  local download_dir="$ext_dir/$(basename ${download_url%.*})" # Remove ext (shortest .* from end)

  if [ ! -d "$download_dir" ]; then # if download_dir missing
    if [ ! -f "$download_file" ]; then # if download_file missing
      LOG "[$log_id] Downloading $title"
      LOG "[$log_id]  - Source: $download_url"
      LOG "[$log_id]  - Target: $download_file"
      curl -L "$download_url" -o "$download_file"
      LOG "[$log_id] Download complete"
    fi

    LOG "[$log_id] Extracting $title"
    LOG "[$log_id]  - Source: $download_file"
    LOG "[$log_id]  - Target: $download_dir"
    unzip -q "$download_file" -d "$download_dir"

    if [ -L "$addon_dir" ]; then
      rm "$addon_dir"
    fi
  fi

  if [ ! -d "$addon_dir" ]; then # if symlink missing
    local symlink_source=$(find "$download_dir" -type d -path "*/$addon_dir" -print -quit)
    if [ ! -d "$symlink_source" ]; then # if source missing
      LOG "[$log_id] *** '$addon_dir' not found - Enjoy content here: ***"
      LOG "[$log_id]  - $download_dir"
    else
      create_symlink="ln -rs $symlink_source $addon_dir"
      LOG "[$log_id] Creating symlink"
      LOG "[$log_id]  - $create_symlink"
      mkdir -p "$(dirname $addon_dir)"
      EXEC $create_symlink
    fi
  fi
}


################
##### MAIN #####
################

main() {
  init_paths

  for repo in "${repos[@]}"; do
    SEP; get_repo $ext_repos $repo
  done

  for addon in "${addons[@]}"; do
    SEP; get_addon $ext_addons $addon
  done

  for repo in "${externals[@]}"; do
    SEP; get_repo $ext_externals $repo
  done
}


###############
##### END #####
###############

main
pause

