# Packages a pre-built publish output; see .github/workflows/build.yml
FROM mcr.microsoft.com/dotnet/runtime-deps:8.0

ARG VERSION
ARG REVISION

ENTRYPOINT ["/opt/helpmebot/docker-entrypoint.sh"]
CMD ["bot"]

USER 1000

ADD helpmebot.tar.gz /opt

LABEL org.opencontainers.image.source="https://github.com/helpmebot/helpmebot" \
    org.opencontainers.image.revision="${REVISION}" \
    org.opencontainers.image.version="${VERSION}"
