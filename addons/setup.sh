#!/bin/bash

# Could remove this line to run without admin/developer mode and copy files instead of symlink
export MSYS=winsymlinks:nativestrict


#############################################################
### Run this script to download/clone external components ###
### Requires Admin or Developer Mode to create symlinks   ###
###  - Downloads & extracts addons from asset library     ###
###  - Clones & pulls external git repos                  ###
###  - Creates symlinks to addon folder                   ###
### By default, addons and externals are ignored          ###
### But addon/zip files could be committed to fix version ###
#############################################################


repos=(
  "https://github.com/Cat-Lips/F00F.Core.git F00F.Core"
  "https://github.com/Cat-Lips/F00F.ShaderNoise.git F00F.ShaderNoise"
  #"https://github.com/Cat-Lips/F00F.ShaderTerrain.git F00F.ShaderTerrain"
)


##################
##### UTILS ######
##################

asset_api="https://godotengine.org/asset-library/api"
ext_addons=".ext/.addons"
ext_repos=".ext/.repos"

init_paths() {
  local root_addons_folder=$(realpath --relative-to="$PWD" "${PWD%%addons*}/addons")
  ext_addons="$root_addons_folder/$ext_addons"
  ext_repos="$root_addons_folder/$ext_repos"
  mkdir -p "$ext_addons"
  mkdir -p "$ext_repos"
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

unzip_file() {
  local log_id=$1
  local zip_file=$2
  local target_dir=$3
  LOG "[$log_id] Extracting $zip_file"
  LOG "[$log_id]  - to $target_dir"
  unzip -q $zip_file -d "$target_dir"
}

download_addon() {
  local log_id=$1
  local asset_id=$2
  local external_dir=$3
  local asset_url="$asset_api/asset/$asset_id"

  LOG "[$log_id] Requesting $asset_url"
  local json=$(curl -sS "$asset_url")
  local title=$(parse_json "$json" title)
  local download_url=$(unescape $(parse_json "$json" download_url))
  local download_file="$external_dir.${download_url##*.}" # ie, $external_dir.$ext

  LOG "[$log_id] Downloading $title"
  LOG "[$log_id]  - $download_url"
  LOG "[$log_id]  - to $download_file"
  curl -L "$download_url" -o "$download_file"
  LOG "[$log_id] Download complete"

  RET "$download_file"
}

create_symlink() {
  local log_id=$1
  local addon_folder=$2
  local external_dir=$3
  local log_if_not_addon=$4
  if [ ! -d "$addon_folder" ]; then # if no addon folder
    local external_addons_dir=$(find "$external_dir" -type d -name "addons" -print -quit)
    if [ -z "$external_addons_dir" ]; then # if no external addon folder
      if [ -n "$log_if_not_addon" ]; then # log on request
        LOG "[$log_id] *** Not an addon - Enjoy content here: ***"
        LOG "[$log_id]  - $external_dir"
      fi
    else
      create_symlink="ln -rs $external_addons_dir/$addon_folder $addon_folder"
      LOG "[$log_id] Creating symlink"
      LOG "[$log_id]  - $create_symlink"
      EXEC $create_symlink
    fi
  fi
}

get_addon() {
  local asset_id=$1
  local addon_folder=$2
  local log_id="$asset_id"
  local external_dir="$ext_addons/$addon_folder"

  if [ ! -d "$external_dir" ]; then # if external dir missing
    local download_file=$(find $ext_addons -maxdepth 1 -type f -name "$addon_folder.*" -print -quit)
    if [ -z "$download_file" ]; then # if download file missing
      download_file=$(download_addon "$log_id" "$asset_id" "$external_dir")
    fi
    unzip_file "$log_id" "$download_file" "$external_dir"
    local log_if_not_addon="True"
  fi

  create_symlink "$log_id" "$addon_folder" "$external_dir" "$log_if_not_addon"
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
  local git_url=$1
  local addon_folder=$2
  local checkout_target=$3
  local log_id="(git) $addon_folder"
  local external_dir="$ext_repos/$addon_folder"

  if [ -d "$external_dir/.git" ]; then # if existing repo
    #local updated=$(git_update "$log_id" "$external_dir" "$checkout_target")
    git_update "$log_id" "$external_dir" "$checkout_target"
    if [ $? -eq 1 ]; then local updated="X"; fi
  else
    #local updated=$(git_clone "$log_id" "$git_url" "$external_dir" "$checkout_target")
    git_clone "$log_id" "$git_url" "$external_dir" "$checkout_target"
    if [ $? -eq 1 ]; then local updated="X"; fi
  fi

  create_symlink "$log_id" "$addon_folder" "$external_dir" "$updated"
}


################
##### MAIN #####
################

main() {
  init_paths

  for addon in "${addons[@]}"; do
    SEP; get_addon $addon
  done

  for repo in "${repos[@]}"; do
    SEP; get_repo $repo
  done
}


###############
##### END #####
###############

main
pause
