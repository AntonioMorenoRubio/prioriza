{
  description = "Dev shell dotnet 10";

  inputs.nixpkgs.url = "github:NixOS/nixpkgs/nixos-25.11";

  outputs = { self, nixpkgs, ... }:
  let
    system = "x86_64-linux";
    pkgs = import nixpkgs { inherit system; };
  in {
    devShells.${system}.default = pkgs.mkShell {
      buildInputs = [
        pkgs.dotnet-sdk_10
      ];

      DOTNET_ROOT = "${pkgs.dotnet-sdk_10}/share/dotnet";
    };
  };
}
